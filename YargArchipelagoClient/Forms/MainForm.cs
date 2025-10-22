using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using System.Collections.Concurrent;
using System.Diagnostics;
using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Helpers;
using YargArchipelagoCommon;
using static YargArchipelagoClient.Data.ArchipelagoColorHelper;
using static YargArchipelagoClient.Helpers.WinFormHelpers;

namespace YargArchipelagoClient
{
    public partial class MainForm : Form
    {
        // Fields using target-typed new expressions
        public ConnectionData Connection;
        public ConfigData Config;

        private readonly ConcurrentQueue<ColoredString> LogQueue = [];
        private readonly SemaphoreSlim LogSignal = new(0);
        private readonly CancellationTokenSource LogCancellation = new();

        private readonly System.Windows.Forms.Timer SyncTimer = new();

        private bool UpdateData = false;

        public MainForm()
        {
            InitializeComponent();
            lvSongList_Resize(this, new());
        }
        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (!ClientInitializationHelper.ConnectToServer(out var connectResult))
            {
                Close();
                return;
            }
            Connection = connectResult!;
            File.WriteAllText(ConnectionForm.ConnectionCachePath, Connection.ToFormattedJson());

            if (!ClientInitializationHelper.GetConfig(Connection, out var configResult))
            {
                Close();
                return;
            }
            Config = configResult!;
            ClientInitializationHelper.ReadSlotData(Connection, Config);
            Config.SaveConfigFile(Connection);

            Debug.WriteLine($"The Following Songs were not valid for any profile in this config\n\n{Config.GetUnusableSongs().Select(x => x.GetSongDisplayName()).ToFormattedJson()}");

            // Subscribe to session events.
            Connection!.GetSession().Items.ItemReceived += Items_ItemReceived;
            Connection!.GetSession().MessageLog.OnMessageReceived += MessageLog_OnMessageReceived;
            Connection!.GetSession().Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;
            Connection.DeathLinkService!.OnDeathLinkReceived += DeathLinkService_OnDeathLinkReceived;

            UpdateData = true;

            var PacketServer = Connection.CreatePacketServer(Config);
            PacketServer.PacketServerClosed += PackerServerClosed;
            PacketServer.ConnectionChanged += UpdateConnected;
            PacketServer.CurrentSongUpdated += UpdateCurrentlyPlaying;
            PacketServer.LogMessage += WriteToLog;
            _ = PacketServer.StartAsync();

            SyncTimerTick(sender, e);

            SyncTimer.Interval = 200;
            SyncTimer.Tick += SyncTimerTick;
            SyncTimer.Start();

            Task.Run(ProcessLogQueueAsync);

            UpdateClientTitle();
        }

        private void PackerServerClosed(string obj)
        {
            MessageBox.Show($"The YARG connection service was stopped unexpectedly, the application will close\n\n{obj}");
            this.Close();
        }
        private void APServerClosed(string obj)
        {
            MessageBox.Show($"The Archipelago connection service was stopped unexpectedly, the application will close\n\n{obj}");
            this.Close();
        }

        private void DeathLinkService_OnDeathLinkReceived(DeathLink deathLink)
        {
            if (!Config!.deathLinkEnabled) return;
            if (Config.ManualMode)
                MessageBox.Show($"Deathlink {deathLink.Source}", deathLink.Cause, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            else
            {
                _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket
                {
                    deathLinkData = new CommonData.DeathLinkData { Source = deathLink.Source, Cause = deathLink.Cause }
                });
                WriteToLog($"Deathlink {deathLink.Source}: {deathLink.Cause}");
            }

        }

        public const string Title = "Yarg Archipelago Client";
        public bool IsConnectedToYarg = false;

        public void UpdateClientTitle()
        {
            string CurrentTitle = Title;
            CurrentTitle += IsConnectedToYarg ? " [Game Connected]" : " [Game Disconnected]";
            if (Connection.CurrentlyPlaying is not null)
                CurrentTitle += $" [Currently Playing: {Connection.CurrentlyPlaying.GetSongDisplayName()}]";

            this.Text = CurrentTitle;
        }
        public void UpdateCurrentlyPlaying(CommonData.SongData? Song)
        {
            Connection.CurrentlyPlaying = Song;
            UpdateClientTitle();
        }
        public void UpdateConnected(bool Connected)
        {
            IsConnectedToYarg = Connected;
            UpdateClientTitle();
            if (Connected)
                CheckLocationHelpers.SendAvailableSongUpdate(Config, Connection);
        }

        private void SyncTimerTick(object? sender, EventArgs e)
        {
            if (!Connection.GetSession().Socket.Connected)
                APServerClosed("AP server connection lost");
            if (!UpdateData) return;
            UpdateData = false;
            Connection.UpdateCheckedLocations();
            Connection.UpdateReceivedItems();
            TrapFillerHelper.SendPendingTrapOrFiller(Connection, Config);
            PrintSongs();
            CheckLocationHelpers.SendAvailableSongUpdate(Config, Connection);
        }

        private void lvSongList_Resize(object sender, EventArgs e) =>
            columnSongName.Width = lvSongList.ClientSize.Width - columnID.Width;

        private void lvSongList_DoubleClick(object sender, EventArgs e)
        {
            if (!Config!.ManualMode) return;
            if (lvSongList.SelectedItems == null || lvSongList.SelectedItems.Count == 0) return;
            var songLocations = lvSongList.SelectedItems.Cast<ListViewItem>().Select(x => x.Tag).OfType<SongLocation>();
            if (!songLocations.Any()) return;
            CheckLocationHelpers.CheckLocations(Config!, Connection, songLocations, ModifierKeys == Keys.Control);
        }


        private void Locations_CheckedLocationsUpdated(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations) => 
            UpdateData = true;

        private void Items_ItemReceived(Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper helper) => 
            UpdateData = true;

        private void MessageLog_OnMessageReceived(LogMessage message)
        {
            var formattedMessage = new ColoredString();
            foreach (var part in message.Parts)
                formattedMessage.AddText(part.Text, part.Color.ConvertToSystemColor(), false);

            if (message is ItemSendLogMessage ItemLog)
            {
                //BroadcastSongNameToServer(ItemSend);
                if (Config.InGameItemLog == CommonData.ItemLog.All || (Config.InGameItemLog == CommonData.ItemLog.ToMe && ItemLog.IsReceiverTheActivePlayer))
                    _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket { Message = message.ToString() });
            }
            if (message is ChatLogMessage && Config.InGameAPChat)
                _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket { Message = message.ToString() });

            LogQueue.Enqueue(formattedMessage);
            LogSignal.Release();
        }

        private void BroadcastSongNameToServer(ItemSendLogMessage message)
        {
            if (!message.IsReceiverTheActivePlayer) return;
            if (!APWorldData.APIDs.SongItemIds.TryGetValue(message.Item.ItemId, out var SongNum))
                return;
            if (!Config!.ApLocationData.TryGetValue(SongNum, out var Song))
                return;
            string SongMessage = Song.GetSongDisplayName(Config!, true, true, true);
            if (Config.BroadcastSongName)
                Connection.GetSession().Say(SongMessage);
            else
                LogQueue.Enqueue(new ColoredString().AddText(SongMessage));
        }

        private async Task ProcessLogQueueAsync()
        {
            while (!LogCancellation.Token.IsCancellationRequested)
            {
                await LogSignal.WaitAsync(LogCancellation.Token);
                //Let a few messages accumulate for cases like releases
                await Task.Delay(300, LogCancellation.Token);
                List<ColoredString> messages = [];
                while (LogQueue.TryDequeue(out var msg))
                    messages.Add(msg);
                if (messages.Count > 0)
                    lbConsole.SafeInvoke(() => lbConsole.AppendString([.. messages]));
            }
        }

        public void WriteToLog(string message)
        {
            LogQueue.Enqueue(new ColoredString(message));
            LogSignal.Release();
        }

        public void PrintSongs()
        {
            lvSongList.SafeInvoke(lvSongList.Items.Clear);

            if (Config.GoalSong.SongAvailableToPlay(Connection, Config))
            {
                ListViewItem goalItem = new(Config.GoalSong.SongNumber.ToString()) { Tag = Config!.GoalSong };
                goalItem.SubItems.Add($"Goal Song: {Config.GoalSong.GetSongDisplayName(Config!)} [{Config.GoalSong.Requirements!.Name}]");
                lvSongList.SafeInvoke(() => lvSongList.Items.Add(goalItem));
            }

            foreach (var i in Config!.ApLocationData.OrderBy(x => x.Key))
            {
                if (!i.Value.SongAvailableToPlay(Connection, Config))
                    continue;
                if (!string.IsNullOrWhiteSpace(txtFilter.Text) && !i.Value.GetSongDisplayName(Config!).Contains(txtFilter.Text))
                    continue;

                ListViewItem item = new(i.Value.SongNumber.ToString()) { Tag = i.Value };
                item.SubItems.Add($"{i.Value.GetSongDisplayName(Config!)} [{i.Value.Requirements!.Name}]");

                lvSongList.SafeInvoke(() => lvSongList.Items.Add(item));
            }
        }

        private void txtFilter_TextChanged(object sender, EventArgs e) => PrintSongs();


        private void btnSendChat_Click(object sender, EventArgs e)
        {
            string Text = textBox1.Text;
            if (Text.IsNullOrWhiteSpace()) return;
            textBox1.Text = string.Empty;
            if (Text.Length > 1 && Text[0] == '/')
                ProcessCommand(Text[1..]);
            else
                Connection.GetSession().Say(Text);

            LogSignal.Release();
        }

        private void ProcessCommand(string v)
        {
            switch (v.ToLower())
            {
                case "broadcast":
                    if (Config is null) return;
                    Config.BroadcastSongName = !Config.BroadcastSongName;
                    Config.SaveConfigFile(Connection);
                    lbConsole.AppendString($"Broadcasting Songs: {Config.BroadcastSongName}");
                    break;
                case "manual":
                    if (Config is null) return;
                    Config.ManualMode = !Config.ManualMode;
                    Config.SaveConfigFile(Connection);
                    lbConsole.AppendString($"Manual Mode: {Config.ManualMode}");
                    break;
                case "deathlink":
                    if (Config is null || !Connection.GetSession().RoomState.ServerTags.Contains("DeathLink")) return;
                    Config.deathLinkEnabled = !Config.deathLinkEnabled;
                    Config.SaveConfigFile(Connection);
                    lbConsole.AppendString($"Deathlink Enabled: {Config.deathLinkEnabled}");
                    break;
                case "fame":
                    lbConsole.AppendString($"Fame Points: {Connection.GetCurrentFame()}/{Config!.FamePointsNeeded}");
                    break;
                case "update":
                    lbConsole.AppendString($"Updating Available");
                    CheckLocationHelpers.SendAvailableSongUpdate(Config, Connection);
                    break;
                case "rescan":
                    lbConsole.AppendString($"Rescanning available songs");
                    SongImporter.RescanSongs(Config, Connection);
                    PrintSongs();
                    break;
                case "debug_star" when Debugger.IsAttached:
                    lbConsole.AppendString($"Simulating start power item");
                    _ = Connection.GetPacketServer()?.SendPacketAsync(new CommonData.Networking.YargAPPacket  { ActionItem = new(CommonData.APActionItem.StarPower)});
                    break;
                case "debug_restart" when Debugger.IsAttached:
                    lbConsole.AppendString($"Simulating restart trap");
                    _ = Connection.GetPacketServer()?.SendPacketAsync(new CommonData.Networking.YargAPPacket { ActionItem = new(CommonData.APActionItem.Restart) });
                    break;
                case "debug_dl" when Debugger.IsAttached:
                    lbConsole.AppendString($"Simulating Death Link");
                    _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket
                    {
                        deathLinkData = new CommonData.DeathLinkData { Source = "Self", Cause = "Command" }
                    });
                    break;
                default:
                    lbConsole.AppendString($"{v} is not a valid command");
                    break;
            }
        }

        private void lvSongList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (lvSongList.HitTest(e.Location).Item is ListViewItem item && item.Tag is SongLocation Song)
                    ContextMenuBuilder.BuildSongMenu(this, Song).Show(MousePosition);
            }
        }
    }
}
