using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using System.Collections.Concurrent;
using System.Diagnostics;
using TDMUtils;
using YargArchipelagoCore.Data;
using YargArchipelagoClient.Helpers;
using YargArchipelagoCommon;
using YargArchipelagoCore.Helpers;
using static YargArchipelagoCore.Data.ArchipelagoColorHelper;
using static YargArchipelagoClient.Helpers.WinFormHelpers;
using YargArchipelagoClient.Forms;

namespace YargArchipelagoClient
{
    public partial class MainForm : Form
    {
        public ConnectionData Connection;
        public ConfigData Config;

        private readonly ConcurrentQueue<ColoredString> LogQueue = [];
        private readonly SemaphoreSlim LogSignal = new(0);
        private readonly CancellationTokenSource LogCancellation = new();

        public EnergyShop? energyShop = null;

        public const string Title = "Yarg Archipelago Client";

        public MainForm()
        {
            InitializeComponent();
            WinFormsMessageBoxTemplate.Apply();
            lvSongList_Resize(this, new());

            cmbDeathLinkMode.Items.AddRange([.. EnumerableUtilities.EnumAsArray<CommonData.DeathLinkType>().Select(x => x.GetDescription())]);
            cmbEnergyLinkMode.Items.AddRange([.. EnumerableUtilities.EnumAsArray<CommonData.EnergyLinkType>().Select(x => x.GetDescription())]);
            cmbItemNotifMode.Items.AddRange([.. EnumerableUtilities.EnumAsArray<CommonData.ItemLog>().Select(x => x.GetDescription())]);

            PreventDropDownCloseOnClick(settingsToolStripMenuItem, deathLinkToolStripMenuItem, energyLinkToolStripMenuItem);
        }

        private void PreventDropDownCloseOnClick(params ToolStripMenuItem[] items)
        {
            energyLinkToolStripMenuItem.DropDown.Closing += (_, e) =>
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                    e.Cancel = true;
            };
        }


        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (!ClientInitializationHelper.ConnectSession(NewConnectionCreator, NewConfigCreator, ApplyConnectionListeners, out Connection, out Config))
            {
                this.Close();
                return;
            }

            Connection!.clientSyncHelper.ConstantCallback += ClientSyncHelper_ConstantCallback;
            Connection!.clientSyncHelper.OnUpdateCallback += ClientSyncHelper_OnUpdateCallback;
            Connection!.clientSyncHelper.APServerClosed += APServerClosed;

            Task.Run(ProcessLogQueueAsync);

            UpdateClientTitle();
            PrintSongs();
        }

        private void ClientSyncHelper_ConstantCallback()
        {
            //txtCurrentEnergyToolStripMenuItem.Text = $"Current Energy: {CheckLocationHelpers.GetEnergy(Connection, Config)}";
        }

        private void ClientSyncHelper_OnUpdateCallback()
        {
            if (InvokeRequired)
                BeginInvoke((Action)(() => ClientSyncHelper_OnUpdateCallback()));
            else
            {
                UpdateClientTitle();
                PrintSongs();
                fame0ToolStripMenuItem.Text = $"Fame: {Connection.GetCurrentFame()} / {Config.FamePointsNeeded}";
            }
        }

        private async void ResetConnection(object sender, EventArgs e)
        {
            Connection!.clientSyncHelper.ConstantCallback -= ClientSyncHelper_ConstantCallback;
            Connection!.clientSyncHelper.OnUpdateCallback -= ClientSyncHelper_OnUpdateCallback;
            Connection!.clientSyncHelper.APServerClosed -= APServerClosed;
            await Connection.DisconnectSession(RemoveConnectionListers);
            Connection = null;
            Config = null;
            if (!ClientInitializationHelper.ConnectSession(NewConnectionCreator, NewConfigCreator, ApplyConnectionListeners, out Connection!, out Config!))
            {
                this.Close();
                return;
            }
            Connection!.clientSyncHelper.OnUpdateCallback += ClientSyncHelper_OnUpdateCallback;
            Connection!.clientSyncHelper.APServerClosed += APServerClosed;

            UpdateClientTitle();
            PrintSongs();
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

        private void ApplyConnectionListeners(ConnectionData connection)
        {
            var PacketServer = connection.GetPacketServer();
            PacketServer.LogMessage += WriteToLog;
            PacketServer.PacketServerError += PackerServerClosed;
            PacketServer.CurrentSongUpdated += UpdateClientTitle;
            PacketServer.ConnectionChanged += UpdateClientTitle;

            var APSession = connection!.GetSession();
            APSession.MessageLog.OnMessageReceived += MessageLog_OnMessageReceived;
            connection.DeathLinkService!.OnDeathLinkReceived += DeathLinkService_OnDeathLinkReceived;
        }
        private void RemoveConnectionListers(ConnectionData connection)
        {
            var PacketServer = connection.GetPacketServer();
            PacketServer.LogMessage -= WriteToLog;
            PacketServer.PacketServerError -= PackerServerClosed;
            PacketServer.CurrentSongUpdated -= UpdateClientTitle;
            PacketServer.ConnectionChanged -= UpdateClientTitle;

            var APSession = connection!.GetSession();
            APSession.MessageLog.OnMessageReceived -= MessageLog_OnMessageReceived;
            connection.DeathLinkService!.OnDeathLinkReceived -= DeathLinkService_OnDeathLinkReceived;
        }

        public void WriteToLog(string message)
        {
            LogQueue.Enqueue(new ColoredString(message));
            LogSignal.Release();
        }
        public void PackerServerClosed(string obj)
        {
            Connection.clientSyncHelper?.StopTimer();
            MessageBox.Show($"The YARG connection service was stopped unexpectedly, the application will close\n\n{obj}");
            ResetConnection(this, null);
        }
        private void MessageLog_OnMessageReceived(LogMessage message)
        {
            var formattedMessage = new ColoredString();
            message.FormatYargItemNames(Config);
            foreach (var part in message.Parts)
                formattedMessage.AddText(part.Text, part.Color.ConvertToSystemColor(), false);

            LogQueue.Enqueue(formattedMessage);
            LogSignal.Release();
        }

        private void DeathLinkService_OnDeathLinkReceived(DeathLink deathLink) => WriteToLog($"Deathlink {deathLink.Source}: {deathLink.Cause}");

        public void UpdateClientTitle()
        {
            string CurrentTitle = Title;

            yARGConnectedToolStripMenuItem.Text = $"YARG Connected: {Connection.IsConnectedToYarg}";
            currentSongToolStripMenuItem.Text = "Current Song: ";
            btnDeathLinkSetToYaml.Text = $"Set to YAML Value: {Config.YamlDeathLink.GetDescription()}";
            btnEnergyLinkSetToYaml.Text = $"Set to YAML Value: {Config.YamlEnergyLink.GetDescription()}";
            if (Connection.IsCurrentlyPlayingSong(out var CurSong))
                currentSongToolStripMenuItem.Text += $" {CurSong!.GetSongDisplayName()}";
            else
                currentSongToolStripMenuItem.Text += $" None";

            aPServerToolStripMenuItem.Text = $"AP Server: {Connection.SlotName}@{Connection.Address}";


            this.Text = CurrentTitle;
        }
        private void APServerClosed(string obj)
        {
            if (InvokeRequired)
                BeginInvoke((Action)(() => APServerClosed(obj)));
            else
            {
                Connection.clientSyncHelper?.StopTimer();
                MessageBox.Show($"The Archipelago connection service was stopped unexpectedly, the application will close\n\n{obj}");
                ResetConnection(this, null);
            }
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
            cmbDeathLinkMode.SelectedIndex = (int)Config.DeathLinkMode;
            cmbEnergyLinkMode.SelectedIndex = (int)Config.EnergyLinkMode;
            cmbItemNotifMode.SelectedIndex = (int)Config.InGameItemLog;
            yARGChatNotificationsToolStripMenuItem.Checked = Config.InGameAPChat;
            DropDownUpdating = false;
        }

        private void OptionDropDownItemChanged(object Sender, EventArgs e)
        {
            if (DropDownUpdating) return;
            Config.BroadcastSongName = broadcastSongNamesToolStripMenuItem.Checked;
            Config.ManualMode = manualModeToolStripMenuItem.Checked;
            Config.DeathLinkMode = Sender == btnDeathLinkSetToYaml ? Config.YamlDeathLink : (CommonData.DeathLinkType)cmbDeathLinkMode.SelectedIndex;
            Config.EnergyLinkMode = Sender == btnEnergyLinkSetToYaml ? Config.YamlEnergyLink : (CommonData.EnergyLinkType)cmbEnergyLinkMode.SelectedIndex;
            Config.InGameItemLog = (CommonData.ItemLog)cmbItemNotifMode.SelectedIndex;
            Config.InGameAPChat = yARGChatNotificationsToolStripMenuItem.Checked;
            Config.SaveConfigFile(Connection);
            Connection.UpdateDeathLinkTags(Config);
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

        private void btnDeathLinkSetToYaml_Click(object sender, EventArgs e)
        {
            OptionDropDownItemChanged(sender, e);
        }

        private void cmbDeathLinkMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            OptionDropDownItemChanged(sender, e);
        }

        private void txtEnergyLinkYamlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionDropDownItemChanged(sender, e);
        }

        private void cmbEnergyLinkMode_SelectedIndexChanged(object sender, EventArgs e)
        {
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

        private void updateAvailableSongsToolStripMenuItem_Click(object sender, EventArgs e) => Connection.GetPacketServer().SendClientStatusPacket();

        private void rescanSongListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SongImporter.RescanSongs(Config, Connection);
            PrintSongs();
        }

        private void openYargAPConfigFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MultiplatformHelpers.OpenFolder(CommonData.DataFolder);
        }

        private void openEnergyLinkShopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (energyShop is null)
                new EnergyShop(this).Show();
            else
                energyShop.Focus();
        }
    }
}
