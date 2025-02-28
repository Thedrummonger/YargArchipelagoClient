using Archipelago.MultiClient.Net.MessageLog.Messages;
using ArchipelagoPowerTools.Data;
using ArchipelagoPowerTools.Helpers;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using TDMUtils;
using YargArchipelagoClient.Data;
using static ArchipelagoPowerTools.Data.ColoredStringHelper;
using static ArchipelagoPowerTools.Helpers.WinFormHelpers;

namespace YargArchipelagoClient
{
    public partial class MainForm : Form
    {
        // Fields using target-typed new expressions
        private ConnectionData Connection;
        private ConfigData? Config;
        private HashSet<int> ReceivedSongs = [];
        private Dictionary<string, int> ReceivedFiller = [];
        private HashSet<long> CheckedLocations = [];
        private int FamePointsNeeded = 0;
        private Dictionary<string, object?> SlotData;

        private readonly ConcurrentQueue<ColoredString> LogQueue = [];
        private readonly SemaphoreSlim LogSignal = new(0);
        private readonly CancellationTokenSource LogCancellation = new();

        private bool UpdateData = false;

        private System.Windows.Forms.Timer SyncTimer = new System.Windows.Forms.Timer();

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

            SlotData = Connection!.GetSession().DataStorage.GetSlotData();

            SyncTimer.Interval = 200;
            SyncTimer.Tick += SyncTimerTick;
            SyncTimer.Start();


            // Subscribe to session events.
            Connection!.GetSession().Items.ItemReceived += Items_ItemReceived;
            Connection!.GetSession().MessageLog.OnMessageReceived += MessageLog_OnMessageReceived;
            Connection!.GetSession().Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;

            if (SlotData["fame_points_for_goal"] is Int64 FPSlotDataval)
                FamePointsNeeded = (int)FPSlotDataval;
            else
                throw new Exception("Could not get Fame Point Goal");

            var SeedDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seeds");
            if (!Directory.Exists(SeedDir))
                Directory.CreateDirectory(SeedDir);

            foreach (var file in Directory.GetFiles(SeedDir))
            {
                var fileName = Path.GetFileName(file);
                Debug.WriteLine($"Checking file {fileName}");
                if (fileName == getSaveFileName())
                {
                    try { Config = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(file)); }
                    catch { Config = null; }
                }
            }

            if (Config is null)
            {
                var configForm = new ConfigForm(Connection!);
                if (configForm.ShowDialog() != DialogResult.OK)
                {
                    Close();
                    return;
                }
                Config = configForm.data!;
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seeds", getSaveFileName()), Config.ToFormattedJson());
            }

            UpdateLocationsChecked();
            UpdateReceivedItems();
            PrintSongs();
            lvSongList_Resize(sender, e);

            Task.Run(() => ProcessLogQueueAsync());

            bool IsConnected() =>
                CForm.Connection is not null &&
                CForm.Connection.GetSession() is not null &&
                CForm.Connection.GetSession()!.Socket.Connected;
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

        private void textBox2_Enter(object sender, EventArgs e) { }

        private void textBox2_Leave(object sender, EventArgs e) { }

        private void lvSongList_Resize(object sender, EventArgs e) =>
            columnSongName.Width = lvSongList.ClientSize.Width - columnID.Width;

        private void lvSongList_DoubleClick(object sender, EventArgs e)
        {
            if (lvSongList.SelectedItems.Count <= 0) return;
            var selectedItems = lvSongList.SelectedItems.Cast<ListViewItem>().ToArray();
            var locationIDs = new List<long>();

            foreach (var item in selectedItems)
            {
                if (item.Tag is not SongLocation songLocation) continue;

                if (songLocation.APFameCheckLocation is long fl && AllChecksComplete(songLocation, []))
                {
                    locationIDs.Add(fl);
                    continue;
                }

                var buttons = new List<CustomMessageResult>();
                int btnCheckCount = 0;
                if (songLocation.APStandardCheckLocation is long sl && !CheckedLocations.Contains(sl))
                {
                    btnCheckCount++;
                    buttons.Add(CustomMessageResult.Reward1);
                }
                if (songLocation.APExtraCheckLocation is long el && !CheckedLocations.Contains(el))
                {
                    btnCheckCount++;
                    buttons.Add(CustomMessageResult.Reward2);
                }
                if (btnCheckCount > 1)
                    buttons.Add(CustomMessageResult.Both);

                var result = ModifierKeys == Keys.Control ? 
                    CustomMessageResult.Both : 
                    APSongMessageBox.Show(
                    $"Check Song {songLocation.SongNumber} [{songLocation.MappedSong}]",
                    $"{songLocation.MappedSong}",
                    [.. buttons]);

                if (result.In(CustomMessageResult.Reward1, CustomMessageResult.Both))
                    locationIDs.Add((long)songLocation.APStandardCheckLocation!);
                if (result.In(CustomMessageResult.Reward2, CustomMessageResult.Both))
                    locationIDs.Add((long)songLocation.APExtraCheckLocation!);

                if (songLocation.APFameCheckLocation is long fl2 && AllChecksComplete(songLocation, locationIDs))
                    locationIDs.Add(fl2);
            }
            Connection!.GetSession().Locations.CompleteLocationChecks([.. locationIDs]);
            UpdateLocationsChecked();
            SyncTimerTick(sender, e);
        }
        bool AllChecksComplete(SongLocation s, List<long> toCheck)
        {
            bool standardComplete = s.APStandardCheckLocation == null ||
                toCheck.Contains(s.APStandardCheckLocation.Value) ||
                CheckedLocations.Contains(s.APStandardCheckLocation.Value);
            bool extraComplete = s.APExtraCheckLocation == null ||
                toCheck.Contains(s.APExtraCheckLocation.Value) ||
                CheckedLocations.Contains(s.APExtraCheckLocation.Value);
            return standardComplete && extraComplete;
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
            if (Song.MappedSong is null) return;
            if (!Config.SongData.TryGetValue(Song.MappedSong, out var SongData)) return;

            string SongMessage = $"[Song {songNum}] {SongData.Name} by {SongData.Artist}";
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
            var allItems = session.Items.AllItemsReceived;
            ReceivedFiller.Clear();

            foreach (var i in allItems)
            {
                var data = i.ItemName.Split(" ");
                if (data.Length == 2 && data[0] == "Song" && int.TryParse(data[1], out var songNum))
                    ReceivedSongs.Add(songNum);
                else
                {
                    if (!ReceivedFiller.ContainsKey(i.ItemName))
                        ReceivedFiller[i.ItemName] = 0;
                    ReceivedFiller[i.ItemName]++;
                }
            }

            if (ReceivedFiller.TryGetValue("Victory", out var v) && v > 0)
                session.SetGoalAchieved();

            Debug.WriteLine($"FP Goal {GetCurrentFame()}/{FamePointsNeeded}");
        }
        private int GetCurrentFame() =>
            ReceivedFiller.TryGetValue("Fame Point", out var famePoints) ? famePoints : 0;

        private void PrintSongs()
        {
            if (lvSongList.InvokeRequired)
                lvSongList.Invoke(new Action(() => lvSongList.Items.Clear()));
            else
                lvSongList.Items.Clear();

            Dictionary<int, SongLocation> songs = Config.ApLocationData;

            if (GetCurrentFame() >= FamePointsNeeded)
            {
                ListViewItem goalItem = new("0")
                {
                    Tag = Config.GoalSong
                };
                goalItem.SubItems.Add(Config.GoalSong.DisplayName);
                if (lvSongList.InvokeRequired)
                    lvSongList.Invoke(new Action(() => lvSongList.Items.Add(goalItem)));
                else
                    lvSongList.Items.Add(goalItem);
            }

            foreach (var i in songs.OrderBy(x => x.Key))
            {
                if (!HasUncheckedLocations(i.Value))
                    continue;
                if (!ReceivedSongs.Contains(i.Value.SongNumber))
                    continue;
                if (!string.IsNullOrWhiteSpace(txtFilter.Text) && !i.Value.DisplayName.Contains(txtFilter.Text))
                    continue;

                ListViewItem item = new(i.Key.ToString())
                {
                    Tag = i.Value
                };
                item.SubItems.Add(i.Value.DisplayName);

                lvSongList.SafeInvoke(() => lvSongList.Items.Add(item));
            }
        }
        private bool HasUncheckedLocations(SongLocation s)
        {
            if (s.APStandardCheckLocation is long sl && !CheckedLocations.Contains(sl))
                return true;
            if (s.APExtraCheckLocation is long el && !CheckedLocations.Contains(el))
                return true;
            if (s.APFameCheckLocation is long fl && !CheckedLocations.Contains(fl))
                return true;
            return false;
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
                    LogQueue.Enqueue(new ColoredString().AddText($"Broadcasting Songs: {Config.BroadcastSongName}"));
                    break;
                default:
                    LogQueue.Enqueue(new ColoredString().AddText($"{v} is not a valid command"));
                    break;
            }
        }
    }
}
