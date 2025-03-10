using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using ArchipelagoPowerTools.Data;
using ArchipelagoPowerTools.Helpers;
using Newtonsoft.Json;
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
        public ConfigData? Config;
        public HashSet<int> ReceivedSongs = [];
        public Dictionary<string, int> ReceivedFiller = [];
        public HashSet<long> CheckedLocations = [];
        public int FamePointsNeeded = 0;
        public Dictionary<string, object?> SlotData;
        public DeathLinkService deathLinkService;
        public bool deathLinkEnabled = false;

        private readonly ConcurrentQueue<ColoredString> LogQueue = [];
        private readonly SemaphoreSlim LogSignal = new(0);
        private readonly CancellationTokenSource LogCancellation = new();

        private bool UpdateData = false;

        private System.Windows.Forms.Timer SyncTimer = new System.Windows.Forms.Timer();

        private System.Windows.Forms.Timer CheckSongTimer = new System.Windows.Forms.Timer();

        public MainForm() => InitializeComponent();

        #region Form Event Handlers

        private void MainForm_Shown(object sender, EventArgs e)
        {
            var CForm = new ConnectionForm();
            while (!IsConnected())
            {
                var result = CForm.ShowDialog(this);
                if (result != DialogResult.OK)
                    break;
            }

            if (!IsConnected())
            {
                Close();
                return;
            }

            Connection = CForm.Connection!;
            deathLinkService = Connection!.GetSession().CreateDeathLinkService();

            File.WriteAllText(CForm.ConnectionCachePath, Connection.ToFormattedJson());

            SlotData = Connection!.GetSession().DataStorage.GetSlotData();

            SyncTimer.Interval = 200;
            SyncTimer.Tick += SyncTimerTick;
            SyncTimer.Start();

            // Subscribe to session events.
            Connection!.GetSession().Items.ItemReceived += Items_ItemReceived;
            Connection!.GetSession().MessageLog.OnMessageReceived += MessageLog_OnMessageReceived;
            Connection!.GetSession().Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;
            deathLinkService.OnDeathLinkReceived += DeathLinkService_OnDeathLinkReceived;

            if (SlotData["fame_points_for_goal"] is Int64 FPSlotDataval)
                FamePointsNeeded = (int)FPSlotDataval;
            else
                throw new Exception("Could not get Fame Point Goal");

            if (SlotData.TryGetValue("death_link", out var DLO) && DLO is Int64 DLI && DLI > 0)
            {
                deathLinkEnabled = true;
                deathLinkService.EnableDeathLink();
            }

            var SeedDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seeds");
            if (!Directory.Exists(SeedDir))
                Directory.CreateDirectory(SeedDir);

            var ConfigFile = Directory.GetFiles(SeedDir).FirstOrDefault(file => Path.GetFileName(file) == getSaveFileName());
            if (ConfigFile is not null)
            {
                Debug.WriteLine($"Seed Found {ConfigFile}");
                try { Config = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(ConfigFile)); }
                catch { Config = null; }
            }

            if (Config is null)
            {
                var configForm = new ConfigForm(Connection!);
                var Result = configForm.ShowDialog();
                if (Result != DialogResult.OK)
                {
                    Close();
                    return;
                }
                Config = configForm.data!;
                UpdateConfigFile();
            }

            UpdateLocationsChecked();
            UpdateReceivedItems();
            PrintSongs();
            lvSongList_Resize(sender, e);

            Task.Run(() => ProcessLogQueueAsync());

            CheckSongTimer.Interval = 1000;
            CheckSongTimer.Tick += CheckSongCompletion;
            CheckSongTimer.Start();

            bool IsConnected() =>
                CForm.Connection is not null &&
                CForm.Connection.GetSession() is not null &&
                CForm.Connection.GetSession()!.Socket.Connected;
        }

        private void DeathLinkService_OnDeathLinkReceived(DeathLink deathLink)
        {
            if (!deathLinkEnabled) return;
            MessageBox.Show(deathLink.Cause, $"DeathLink {deathLink.Source}", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        public void UpdateConfigFile()
        {
            if (Config is null) return;
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seeds", getSaveFileName()), Config.ToFormattedJson());
        }

        private string getSaveFileName()
        {
            var c = Connection!;
            var s = c.GetSession();
            return $"{s.RoomState.Seed}_{c.SlotName}_{s.Players.ActivePlayer.Slot}_{s.Players.ActivePlayer.GetHashCode()}";
        }

        private void SyncTimerTick(object? sender, EventArgs e)
        {
            if (!UpdateData) return;
            UpdateData = false;
            UpdateLocationsChecked();
            UpdateReceivedItems();
            PrintSongs();
        }

        private string LastReadContent = string.Empty;
        private void CheckSongCompletion(object? sender, EventArgs e)
        {
            if (Config!.ManualMode) return;
            try
            {
                if (!Directory.Exists(CommonData.DataFolder) || !File.Exists(CommonData.LastPlayedSong)) return;
                using var stream = File.Open(CommonData.LastPlayedSong, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                string newContent = reader.ReadToEnd();
                if (newContent == LastReadContent) return; // if unchanged, do nothing
                LastReadContent = newContent;

                var passInfo = JsonConvert.DeserializeObject<CommonData.SongPassInfo>(newContent);
                if (passInfo is null) return;
                var TargetSong = Config!.ApLocationData.Values.FirstOrDefault(x => x.SongHash == passInfo!.SongHash);
                if (TargetSong is null || !TargetSong.HasUncheckedLocations(CheckedLocations)) return;

                if (TargetSong.FameCheckAvailable(CheckedLocations, out var FL1))
                {
                    CheckLocations([FL1], [TargetSong]);
                    return;
                }

                HashSet<long> ToCheck = [];
                if (TargetSong.HasStandardCheck(out var SL1) && !CheckedLocations.Contains(SL1)) 
                {
                    if (TargetSong.Requirements!.MetStandard(passInfo, out var SL1DL))
                        ToCheck.Add(SL1);
                    else if (deathLinkEnabled && SL1DL)
                        deathLinkService.SendDeathLink(new(Connection.SlotName, $"Failed {TargetSong.GetSongDisplayName(Config!)}"));
                } 
                if (TargetSong.HasExtraCheck(out var EL1) && !CheckedLocations.Contains(EL1))
                {
                    if (TargetSong.Requirements!.MetExtra(passInfo, out var EL1DL))
                        ToCheck.Add(EL1);
                    else if (deathLinkEnabled && EL1DL)
                        deathLinkService.SendDeathLink(new(Connection.SlotName, $"Failed {TargetSong.GetSongDisplayName(Config!)}"));
                }
                if (ToCheck.Count > 0)
                    CheckLocations(ToCheck, [TargetSong]);
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"Failed to read Last Played Song {ex}");
            }
        }

        private void textBox2_Enter(object sender, EventArgs e) { }

        private void textBox2_Leave(object sender, EventArgs e) { }

        private void lvSongList_Resize(object sender, EventArgs e) =>
            columnSongName.Width = lvSongList.ClientSize.Width - columnID.Width;

        private void lvSongList_DoubleClick(object sender, EventArgs e)
        {
            if (!Config!.ManualMode) return;
            if (lvSongList.SelectedItems.Count <= 0) return;
            var selectedItems = lvSongList.SelectedItems.Cast<ListViewItem>().ToArray();
            var locationIDs = new HashSet<long>();
            var CheckStateChanged = new HashSet<SongLocation>();

            foreach (var item in selectedItems)
            {
                if (item.Tag is not SongLocation songLocation) continue;

                string SongName = songLocation.GetSongData(Config!)?.Name?? songLocation.SongNumber.ToString();
                if (songLocation.FameCheckAvailable(CheckedLocations, out var fl))
                {
                    locationIDs.Add(fl);
                    continue;
                }

                List<long> ToCheck = [];
                var buttons = new List<CustomMessageResult>();
                int btnCheckCount = 0;
                if (songLocation.HasStandardCheck(out long sl) && !CheckedLocations.Contains(sl))
                {
                    btnCheckCount++;
                    buttons.Add(CustomMessageResult.Reward1);
                }
                if (songLocation.HasExtraCheck(out long el) && !CheckedLocations.Contains(el))
                {
                    btnCheckCount++;
                    buttons.Add(CustomMessageResult.Reward2);
                }
                if (btnCheckCount > 1)
                    buttons.Add(CustomMessageResult.Both);

                var result = ModifierKeys == Keys.Control ?
                    CustomMessageResult.Both :
                    APSongMessageBox.Show(
                    $"Check Song {songLocation.GetSongDisplayName(Config!, false, false, true)}",
                    songLocation.GetSongDisplayName(Config!, true, true, false),
                    [.. buttons]);

                if (result.In(CustomMessageResult.Reward1, CustomMessageResult.Both) &&
                    songLocation.HasStandardCheck(out var sl1) &&
                    !CheckedLocations.Contains(sl1))
                    ToCheck.Add(sl1);
                if (result.In(CustomMessageResult.Reward2, CustomMessageResult.Both) &&
                    songLocation.HasExtraCheck(out var el1) &&
                    !CheckedLocations.Contains(el1))
                    ToCheck.Add(el1);

                if (songLocation.FameCheckAvailable([.. CheckedLocations, .. ToCheck], out var fl2) &&
                    !CheckedLocations.Contains(fl2))
                    ToCheck.Add(fl2);

                if (ToCheck.Count > 0) CheckStateChanged.Add(songLocation);
                locationIDs = [.. locationIDs, .. ToCheck];
            }
            CheckLocations(locationIDs, CheckStateChanged);
        }

        public void CheckLocations(IEnumerable<long> Locations, IEnumerable<SongLocation> songLocations)
        {
            if (Config!.BroadcastSongName)
            {
                foreach (var i in songLocations)
                    Connection.GetSession().Say(i.GetSongDisplayName(Config!, true, true, true));
            }
            Connection!.GetSession().Locations.CompleteLocationChecks([.. Locations]);
            UpdateLocationsChecked();
            SyncTimerTick(null, new());
        }

        #endregion

        #region Connection and Session Event Handlers

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
            var data = message.Item.ItemName.Split(" ");
            if (data.Length != 2) return;
            if (data[0] != "Song") return;
            if (!int.TryParse(data[1], out var songNum)) return;
            if (Config is null) return;
            if (!Config.ApLocationData.TryGetValue(songNum, out var Song)) return;

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
                await Task.Delay(500, LogCancellation.Token);
                List<ColoredString> messages = [];
                while (LogQueue.TryDequeue(out var msg))
                    messages.Add(msg);
                if (messages.Count > 0)
                    lbConsole.SafeInvoke(() => ColoredStringHelper.AppendColoredString(lbConsole, [.. messages]));
            }
        }
        #endregion

        #region Update and Helper Functions

        private void UpdateLocationsChecked() =>
            CheckedLocations = [.. Connection!.GetSession().Locations.AllLocationsChecked];

        private void UpdateReceivedItems()
        {
            var session = Connection!.GetSession();
            var allItems = session.Items.AllItemsReceived.Where(x => x.Player == Connection.GetSession().Players.ActivePlayer);
            ReceivedFiller.Clear();

            foreach (var i in allItems)
            {
                var data = i.ItemName.Split(" ");
                if (data.Length == 2 && data[0] == "Song" && int.TryParse(data[1], out var songNum))
                    ReceivedSongs.Add(songNum);
                else
                {
                    ReceivedFiller.SetIfEmpty(i.ItemName, 0);
                    ReceivedFiller[i.ItemName]++;
                }
            }

            if (ReceivedFiller.TryGetValue(CommonData.StaticItems.Victory.GetDescription(), out var v) && v > 0)
                session.SetGoalAchieved();

            Debug.WriteLine($"FP Goal {GetCurrentFame()}/{FamePointsNeeded}");
        }
        private int GetCurrentFame() =>
            ReceivedFiller.TryGetValue(CommonData.StaticItems.FamePoint.GetDescription(), out var famePoints) ? famePoints : 0;

        public void PrintSongs()
        {
            lvSongList.SafeInvoke(lvSongList.Items.Clear);

            Dictionary<int, SongLocation> songs = Config!.ApLocationData;

            if (GetCurrentFame() >= FamePointsNeeded)
            {
                ListViewItem goalItem = new("0") { Tag = Config.GoalSong };
                goalItem.SubItems.Add($"{Config.GoalSong.GetSongDisplayName(Config!)} [{Config.GoalSong.Requirements!.Name}]");
                lvSongList.SafeInvoke(() => lvSongList.Items.Add(goalItem));
            }

            foreach (var i in songs.OrderBy(x => x.Key))
            {
                if (!i.Value.HasUncheckedLocations(CheckedLocations))
                    continue;
                if (!ReceivedSongs.Contains(i.Value.SongNumber))
                    continue;
                if (!string.IsNullOrWhiteSpace(txtFilter.Text) && !i.Value.GetSongDisplayName(Config!).Contains(txtFilter.Text))
                    continue;

                ListViewItem item = new(i.Key.ToString()) { Tag = i.Value };
                item.SubItems.Add($"{i.Value.GetSongDisplayName(Config!)} [{i.Value.Requirements!.Name}]");

                lvSongList.SafeInvoke(() => lvSongList.Items.Add(item));
            }
        }

        #endregion

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
                    UpdateConfigFile();
                    LogQueue.Enqueue(new ColoredString($"Broadcasting Songs: {Config.BroadcastSongName}"));
                    break;
                case "manual":
                    if (Config is null) return;
                    Config.ManualMode = !Config.ManualMode;
                    UpdateConfigFile();
                    LogQueue.Enqueue(new ColoredString($"Manual Mode: {Config.ManualMode}"));
                    break;
                case "fame":
                    LogQueue.Enqueue(new ColoredString($"Fame Points: {GetCurrentFame()}/{FamePointsNeeded}"));
                    break;
                default:
                    LogQueue.Enqueue(new ColoredString($"{v} is not a valid command"));
                    break;
            }
        }

        private void lvSongList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var hit = lvSongList.HitTest(e.Location);
            if (hit.Item is ListViewItem item && item.Tag is SongLocation Song)
                ContextMenuHelper.BuildSongListContextMenu(this, item, Song).Show(lvSongList, e.Location);

        }

    }
}
