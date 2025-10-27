using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using System.Collections.Concurrent;
using System.Diagnostics;
using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Helpers;
using YargArchipelagoCommon;
using YargArchipelagoCore.Helpers;
using static YargArchipelagoClient.Data.ArchipelagoColorHelper;
using static YargArchipelagoClient.Helpers.WinFormHelpers;

namespace YargArchipelagoClient
{
    public partial class MainForm : Form
    {
        public ConnectionData Connection;
        public ConfigData Config;

        private readonly ConcurrentQueue<ColoredString> LogQueue = [];
        private readonly SemaphoreSlim LogSignal = new(0);
        private readonly CancellationTokenSource LogCancellation = new();

        private readonly System.Windows.Forms.Timer SyncTimer = new();

        private bool UpdateData = false;

        public const string Title = "Yarg Archipelago Client";
        public bool IsConnectedToYarg = false;

        public MainForm()
        {
            InitializeComponent();
            SetMultiPlatformDialogBoxAction();
            lvSongList_Resize(this, new());

            settingsToolStripMenuItem.DropDown.Closing += (_, e) =>
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                    e.Cancel = true;
            };
        }


        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (!ConnectToAP(NewConnectionCreator, NewConfigCreator))
                return;
            ConnectPipeServer();
            CreateAppQueues(sender, e);
            UpdateClientTitle();
        }

        private ConnectionData? NewConnectionCreator()
        {
            var CForm = new ConnectionForm();
            var dialog = CForm.ShowDialog();
            if (dialog != DialogResult.OK)
                return null;

            return CForm.Connection;
        }

        private ConfigData? NewConfigCreator()
        {
            var configForm = new ConfigForm(Connection!);
            var Dialog = configForm.ShowDialog();
            if (Dialog != DialogResult.OK)
                return null;
            return configForm.data;
        }

        private bool ConnectToAP(Func<ConnectionData?> CreateNewConnection, Func<ConfigData?> CreateNewConfig)
        {
            if (!ClientInitializationHelper.ConnectToServer(out var connectResult, CreateNewConnection))
            {
                Close();
                return false;
            }
            Connection = connectResult!;
            File.WriteAllText(ConnectionForm.ConnectionCachePath, Connection.ToFormattedJson());

            if (!ClientInitializationHelper.GetConfig(Connection, out var configResult, CreateNewConfig))
            {
                Close();
                return false;
            }
            Config = configResult!;
            Config.SaveConfigFile(Connection);

            Debug.WriteLine($"The Following Songs were not valid for any profile in this config\n\n{Config.GetUnusableSongs().Select(x => x.GetSongDisplayName()).ToFormattedJson()}");

            if (Config.ServerDeathLink)
                Connection.DeathLinkService?.EnableDeathLink();
            Connection!.GetSession().Items.ItemReceived += Items_ItemReceived;
            Connection!.GetSession().MessageLog.OnMessageReceived += MessageLog_OnMessageReceived;
            Connection!.GetSession().Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;
            Connection.DeathLinkService!.OnDeathLinkReceived += DeathLinkService_OnDeathLinkReceived;

            aPServerToolStripMenuItem.Text = $"AP Server: {Connection.SlotName}@{Connection.Address}";
            return true;
        }

        private async void DisconnectFromAP(object sender, EventArgs e)
        {
            SyncTimer.Stop();
            try { await Connection!.GetSession().Socket.DisconnectAsync(); } catch { }
            DisconnectPipeServer();
            Connection = null;
            Config = null;
            ConnectToAP(NewConnectionCreator, NewConfigCreator);
            ConnectPipeServer();
            UpdateClientTitle();
            SyncTimer.Start();
        }

        public void ConnectPipeServer()
        {
            UpdateData = true;
            var PacketServer = Connection!.CreatePacketServer(Config);
            PacketServer.LogMessage += WriteToLog;
            PacketServer.CurrentSongUpdated += UpdateCurrentlyPlaying;
            PacketServer.ConnectionChanged += UpdateConnected;
            PacketServer.PacketServerClosed += APServerClosed;
            _ = PacketServer.StartAsync();
        }
        public void DisconnectPipeServer()
        {
            var PacketServer = Connection!.GetPacketServer();
            PacketServer.LogMessage -= WriteToLog;
            PacketServer.CurrentSongUpdated -= UpdateCurrentlyPlaying;
            PacketServer.ConnectionChanged -= UpdateConnected;
            PacketServer.PacketServerClosed -= APServerClosed;
            PacketServer.Stop();
        }

        public void CreateAppQueues(object sender, EventArgs e)
        {
            SyncTimerTick(sender, e);

            SyncTimer.Interval = 200;
            SyncTimer.Tick += SyncTimerTick;
            SyncTimer.Start();

            Task.Run(ProcessLogQueueAsync);
        }

        public void PackerServerClosed(string obj)
        {
            SyncTimer.Stop();
            MessageBox.Show($"The YARG connection service was stopped unexpectedly, the application will close\n\n{obj}");
            this.Close();
        }
        private void APServerClosed(string obj)
        {
            SyncTimer.Stop();
            MessageBox.Show($"The Archipelago connection service was stopped unexpectedly, the application will close\n\n{obj}");
            DisconnectFromAP(this, null);
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

        public void UpdateClientTitle()
        {
            string CurrentTitle = Title;

            yARGConnectedToolStripMenuItem.Text = $"YARG Connected: {IsConnectedToYarg}";
            currentSongToolStripMenuItem.Text = "Current Song: ";
            if (Connection.CurrentlyPlaying is not null)
                currentSongToolStripMenuItem.Text += $" {Connection.CurrentlyPlaying.GetSongDisplayName()}";
            else
                currentSongToolStripMenuItem.Text += $" None";


            this.Text = CurrentTitle;
        }
        public void UpdateCurrentlyPlaying(CommonData.SongData? Song)
        {
            if (Connection is null) return;
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
            if (Connection is null || Config is null)
                return;
            if (!Connection.GetSession().Socket.Connected)
                APServerClosed("AP server connection lost");
            if (!UpdateData) return;
            UpdateData = false;
            Connection.UpdateCheckedLocations();
            Connection.UpdateReceivedItems();
            TrapFillerHelper.SendPendingTrapOrFiller(Connection, Config);
            PrintSongs();
            CheckLocationHelpers.SendAvailableSongUpdate(Config, Connection);
            fame0ToolStripMenuItem.Text = $"Fame: {Connection.GetCurrentFame()} / {Config.FamePointsNeeded}";
        }

        private void lvSongList_Resize(object sender, EventArgs e) =>
            columnSongName.Width = lvSongList.ClientSize.Width - columnID.Width;

        private void lvSongList_DoubleClick(object sender, EventArgs e)
        {
            if (!Config!.ManualMode) return;
            if (lvSongList.SelectedItems == null || lvSongList.SelectedItems.Count == 0) return;
            var songLocations = lvSongList.SelectedItems.Cast<ListViewItem>().Select(x => x.Tag).OfType<SongLocation>();
            if (!songLocations.Any()) return;
            WinFormCheckLocationHelpers.CheckLocations(Config!, Connection, songLocations, ModifierKeys == Keys.Control);
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
                if (Config.InGameItemLog == CommonData.ItemLog.All || (Config.InGameItemLog == CommonData.ItemLog.ToMe && ItemLog.IsReceiverTheActivePlayer))
                    _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket { Message = message.ToString() });
            }
            if (message is ChatLogMessage && Config.InGameAPChat)
                _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket { Message = message.ToString() });

            LogQueue.Enqueue(formattedMessage);
            LogSignal.Release();
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

            var GoalSongAvailable = Config.GoalSong.SongAvailableToPlay(Connection, Config);
            if (GoalSongAvailable || Config.DebugPrintAllSongs)
            {
                string Debug = Config.DebugPrintAllSongs ? !Config.GoalSong.HasUncheckedLocations(Connection) ? "@ " : (GoalSongAvailable ? "O " : "X ") : "";
                ListViewItem goalItem = new(Config.GoalSong.SongNumber.ToString()) { Tag = Config!.GoalSong };
                goalItem.SubItems.Add($"{Debug}Goal Song: {Config.GoalSong.GetSongDisplayName(Config!)} [{Config.GoalSong.Requirements!.Name}]");
                lvSongList.SafeInvoke(() => lvSongList.Items.Add(goalItem));
            }

            foreach (var i in Config!.ApLocationData.OrderBy(x => x.Key))
            {
                var SongAvailable = i.Value.SongAvailableToPlay(Connection, Config);
                if (!SongAvailable && !Config.DebugPrintAllSongs)
                    continue;
                if (!string.IsNullOrWhiteSpace(txtFilter.Text) && !i.Value.GetSongDisplayName(Config!).Contains(txtFilter.Text))
                    continue;

                string Debug = Config.DebugPrintAllSongs ? !i.Value.HasUncheckedLocations(Connection) ? "@ " : SongAvailable ? "O " : "X " : "";
                ListViewItem item = new(i.Value.SongNumber.ToString()) { Tag = i.Value };
                item.SubItems.Add($"{Debug}{i.Value.GetSongDisplayName(Config!)} [{i.Value.Requirements!.Name}]");

                lvSongList.SafeInvoke(() => lvSongList.Items.Add(item));
            }
        }

        private void txtFilter_TextChanged(object sender, EventArgs e) => PrintSongs();


        private void btnSendChat_Click(object sender, EventArgs e)
        {
            string Text = textBox1.Text;
            if (Text.IsNullOrWhiteSpace()) return;
            textBox1.Text = string.Empty;
            if (Text.Length > 1 && Text[0] == '/' && Debugger.IsAttached)
                LocalCommandProcessor.ProcessCommand(Config, Connection, WriteToLog, PrintSongs, Text[1..]);
            else
                Connection.GetSession().Say(Text);

            LogSignal.Release();
        }

        private void lvSongList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (lvSongList.HitTest(e.Location).Item is ListViewItem item && item.Tag is SongLocation Song)
                    ContextMenuBuilder.BuildSongMenu(this, Song).Show(MousePosition);
            }
        }

        bool DropDownUpdating = false;
        private void UpdatedDropDownChecks(object Sender, EventArgs e)
        {
            DropDownUpdating = true;
            broadcastSongNamesToolStripMenuItem.Checked = Config.BroadcastSongName;
            manualModeToolStripMenuItem.Checked = Config.ManualMode;
            deathLinkToolStripMenuItem.Enabled = Config.ServerDeathLink;
            deathLinkToolStripMenuItem.Checked = Config.deathLinkEnabled;
            cmbItemNotifMode.SelectedIndex = (int)Config.InGameItemLog;
            yARGChatNotificationsToolStripMenuItem.Checked = Config.InGameAPChat;
            DropDownUpdating = false;
        }

        private void OptionDropDownItemChanged(object Sender, EventArgs e)
        {
            if (DropDownUpdating) return;
            Config.BroadcastSongName = broadcastSongNamesToolStripMenuItem.Checked;
            Config.ManualMode = manualModeToolStripMenuItem.Checked;
            Config.ServerDeathLink = deathLinkToolStripMenuItem.Checked;
            Config.InGameItemLog = (CommonData.ItemLog)cmbItemNotifMode.SelectedIndex;
            Config.InGameAPChat = yARGChatNotificationsToolStripMenuItem.Checked;
            Config.SaveConfigFile(Connection);
            UpdatedDropDownChecks(Sender, e);
        }

        private void broadcastSongNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            broadcastSongNamesToolStripMenuItem.Checked = !broadcastSongNamesToolStripMenuItem.Checked;
            OptionDropDownItemChanged(sender, e);
        }

        private void manualModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            manualModeToolStripMenuItem.Checked = !manualModeToolStripMenuItem.Checked;
            OptionDropDownItemChanged(sender, e);
        }

        private void deathLinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deathLinkToolStripMenuItem.Checked = !deathLinkToolStripMenuItem.Checked;
            OptionDropDownItemChanged(sender, e);
        }

        private void cmbItemNotifMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            OptionDropDownItemChanged(sender, e);
        }

        private void yARGChatNotificationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            yARGChatNotificationsToolStripMenuItem.Checked = !yARGChatNotificationsToolStripMenuItem.Checked;
            OptionDropDownItemChanged(sender, e);
        }

        private void updateAvailableSongsToolStripMenuItem_Click(object sender, EventArgs e) => CheckLocationHelpers.SendAvailableSongUpdate(Config, Connection);

        private void rescanSongListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SongImporter.RescanSongs(Config, Connection);
            PrintSongs();
        }
    }
}
