using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using ArchipelagoPowerTools.Data;
using ArchipelagoPowerTools.Helpers;
using System.Collections.Concurrent;
using System.Diagnostics;
using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Helpers;
using static ArchipelagoPowerTools.Data.ColoredStringHelper;
using static ArchipelagoPowerTools.Helpers.WinFormHelpers;

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
        private readonly System.Windows.Forms.Timer CheckSongTimer = new();

        private APPacketServer PacketServer;

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

            // Subscribe to session events.
            Connection!.GetSession().Items.ItemReceived += Items_ItemReceived;
            Connection!.GetSession().MessageLog.OnMessageReceived += MessageLog_OnMessageReceived;
            Connection!.GetSession().Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;
            Connection.DeathLinkService!.OnDeathLinkReceived += DeathLinkService_OnDeathLinkReceived;

            UpdateData = true;
            SyncTimerTick(sender, e);

            SyncTimer.Interval = 200;
            SyncTimer.Tick += SyncTimerTick;
            SyncTimer.Start();

            //CheckSongTimer.Interval = 1000;
            //CheckSongTimer.Tick += CheckSongCompletion;
            //CheckSongTimer.Start();

            Task.Run(ProcessLogQueueAsync);
            PacketServer = new APPacketServer(Config, Connection, WriteToLog, UpdateCurrentlyPlaying, UpdateConnected);
            _ = PacketServer.StartAsync();
            UpdateClientTitle();
        }


        private void DeathLinkService_OnDeathLinkReceived(DeathLink deathLink)
        {
            if (!Config!.deathLinkEnabled) return;
            if (Config.ManualMode)
                MessageBox.Show(deathLink.Cause, $"DeathLink {deathLink.Source}", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            else
            {
                _ = PacketServer.SendPacketAsync(new CommonData.Networking.ClientDataPacket { 
                    deathLinkData = new CommonData.DeatLinkData { Source = deathLink.Source, Cause = deathLink.Cause } 
                });
                WriteToLog(deathLink.Source);
            }

        }
        private void CheckForRestartTrap()
        {
            if (!Connection.ReceivedFiller.TryGetValue(APWorldData.StaticItems.TrapRestart, out var RestartTrapAmountFromServer))
                return;

            var RestartTrapAmountInMemory = Config.TrapsRegistered.TryGetValue(APWorldData.StaticItems.TrapRestart, out var R) ? R : 0;

            if (RestartTrapAmountFromServer > RestartTrapAmountInMemory)
                _ = PacketServer.SendPacketAsync(new CommonData.Networking.ClientDataPacket { trapData = new CommonData.TrapData(CommonData.trapType.Restart) });

            if (RestartTrapAmountInMemory != RestartTrapAmountFromServer)
            {
                Config.TrapsRegistered[APWorldData.StaticItems.TrapRestart] = RestartTrapAmountFromServer;
                Config.SaveConfigFile(Connection);
            }

        }

        public const string Title = "Yarg Archipelago Client";
        public CommonData.SongData? CurrentlyPlaying = null;
        public bool IsConnectedToYarg = false;

        public void UpdateClientTitle()
        {
            string CurrentTitle = Title;
            CurrentTitle += $" (Yarg Connected: {IsConnectedToYarg})";
            if (CurrentlyPlaying is not null)
                CurrentTitle += $" [Currently Playing: {CurrentlyPlaying.Name} by {CurrentlyPlaying.Artist}]";

            this.Text = CurrentTitle;
        }
        public void UpdateCurrentlyPlaying(string SongHash)
        {
            if (string.IsNullOrWhiteSpace(SongHash))
                CurrentlyPlaying = null;
            if (Config.SongData.TryGetValue(SongHash, out var song))
                CurrentlyPlaying = null;
            CurrentlyPlaying = song;
            UpdateClientTitle();
        }
        public void UpdateConnected(bool Connected)
        {
            IsConnectedToYarg = Connected;
            UpdateClientTitle();
        }


        private void SyncTimerTick(object? sender, EventArgs e)
        {
            if (!UpdateData) return;
            UpdateData = false;
            Connection.UpdateCheckedLocations();
            Connection.UpdateReceivedItems();
            CheckForRestartTrap();
            PrintSongs();
        }

        private void CheckSongCompletion(object? sender, EventArgs e)
        {
            if (Config!.ManualMode) return;
            CheckLocationHelpers.CheckLocations(Config!, Connection);
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
                formattedMessage.AddText(part.Text, part.Color, false);

            if (message is ItemSendLogMessage ItemSend)
                BroadcastSongNameToServer(ItemSend);

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
                    lbConsole.SafeInvoke(() => ColoredStringHelper.AppendColoredString(lbConsole, [.. messages]));
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

            if (Connection.HasFamePointGoal(Config!))
            {
                ListViewItem goalItem = new("0") { Tag = Config!.GoalSong };
                goalItem.SubItems.Add($"[Goal] {Config.GoalSong.GetSongDisplayName(Config!)} [{Config.GoalSong.Requirements!.Name}]");
                lvSongList.SafeInvoke(() => lvSongList.Items.Add(goalItem));
            }

            foreach (var i in Config!.ApLocationData.OrderBy(x => x.Key))
            {

                if (!i.Value.SongAvailableToPlay(Connection))
                    continue;
                if (!string.IsNullOrWhiteSpace(txtFilter.Text) && !i.Value.GetSongDisplayName(Config!).Contains(txtFilter.Text))
                    continue;

                ListViewItem item = new(i.Key.ToString()) { Tag = i.Value };
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
                    WriteToLog($"Broadcasting Songs: {Config.BroadcastSongName}");
                    break;
                case "manual":
                    if (Config is null) return;
                    Config.ManualMode = !Config.ManualMode;
                    Config.SaveConfigFile(Connection);
                    WriteToLog($"Manual Mode: {Config.ManualMode}");
                    break;
                case "deathlink":
                    if (Config is null || !Connection.GetSession().RoomState.ServerTags.Contains("DeathLink")) return;
                    Config.deathLinkEnabled = !Config.deathLinkEnabled;
                    Config.SaveConfigFile(Connection);
                    WriteToLog($"Deathlink Enabled: {Config.deathLinkEnabled}");
                    break;
                case "fame":
                    WriteToLog($"Fame Points: {Connection.GetCurrentFame()}/{Config!.FamePointsNeeded}");
                    break;
                default:
                    WriteToLog($"{v} is not a valid command");
                    break;
            }
        }

        private void lvSongList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var hit = lvSongList.HitTest(e.Location);
            if (hit.Item is ListViewItem item && item.Tag is SongLocation Song)
                new ContextMenuBuilder(this, Song).BuildSongListContextMenu().Show();

        }
    }
}
