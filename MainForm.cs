using ArchipelagoPowerTools.Data;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Forms;
using TDMUtils;
using YargArchipelagoClient.Data;

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

        private List<ColoredString> Log = [];

        private bool UpdateData = false;

        private System.Windows.Forms.Timer SyncTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer LogTimer = new System.Windows.Forms.Timer();

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

            LogTimer.Interval = 500;
            LogTimer.Tick += LogTimerTick;
            LogTimer.Start();


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

            foreach(var file in Directory.GetFiles(SeedDir))
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


        private void LogTimerTick(object? sender, EventArgs e)
        {
            if (Log.Count < 1) return;
            ColoredString[] TempLog = [.. Log];
            Log.Clear();
            if (lbConsole.InvokeRequired)
                lbConsole.Invoke(new Action(() => ColoredStringHelper.AppendColoredString(lbConsole, TempLog)));
            else
                ColoredStringHelper.AppendColoredString(lbConsole, TempLog);
        }

        private void textBox2_Enter(object sender, EventArgs e) { }

        private void textBox2_Leave(object sender, EventArgs e) { }

        private void lvSongList_Resize(object sender, EventArgs e) =>
            columnSongName.Width = lvSongList.ClientSize.Width - columnID.Width;

        private void lvSongList_DoubleClick(object sender, EventArgs e)
        {
            if (lvSongList.SelectedItems.Count <= 0) return;

            ListViewItem[] selectedItems = lvSongList.SelectedItems.Cast<ListViewItem>().ToArray();
            List<long> locationIDs = new();

            foreach (var i in selectedItems)
            {
                if (i.Tag is not SongLocation songLocation)
                    continue;
                if (songLocation.APStandardCheckLocation is long sl)
                    locationIDs.Add(sl);
                if (songLocation.APExtraCheckLocation is long el)
                    locationIDs.Add(el);
                if (songLocation.APFameCheckLocation is long fl)
                    locationIDs.Add(fl);
            }
            Connection!.GetSession().Locations.CompleteLocationChecks(locationIDs.ToArray());
            SyncTimerTick(sender, e);
        }

        #endregion

        #region Connection and Session Event Handlers

        private void Locations_CheckedLocationsUpdated(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations) => 
            UpdateData = true;

        private void Items_ItemReceived(Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper helper) =>
            UpdateData = true;

        private void MessageLog_OnMessageReceived(Archipelago.MultiClient.Net.MessageLog.Messages.LogMessage message)
        {
            var formattedMessage = new ColoredString();
            foreach (var part in message.Parts)
                formattedMessage.AddText(part.Text, part.Color, false);
            Log.Add(formattedMessage);
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

                if (lvSongList.InvokeRequired)
                    lvSongList.Invoke(new Action(() => lvSongList.Items.Add(item)));
                else
                    lvSongList.Items.Add(item);
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
    }
}
