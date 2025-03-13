using ArchipelagoPowerTools.Data;
using Newtonsoft.Json;
using System.Data;
using YargArchipelagoClient.Data;
using static YargArchipelagoClient.Data.CommonData;

namespace YargArchipelagoClient
{
    public partial class ConfigForm : Form
    {
        #region Fields

        public ConfigData data;
        private ConnectionData Connection;
        private List<SongPool> Pools = [];

        bool PoolUpdating = false;

        bool SongError = false;

        #endregion

        #region Constructor

        public ConfigForm(ConnectionData connection)
        {
            InitializeComponent();
            Connection = connection;
            cmbSelectedPool.DataSource = Pools;
            data = new ConfigData();
            data.ParseAPLocations(connection.GetSession());
            if (!TryReadSongs(out var SongData))
            {
                DialogResult = DialogResult.Abort;
                SongError = true;
                return;
            }
            data.SongData = SongData;
            gbCurrentPool.Enabled = false;
            gbSongPoolSelect.Enabled = Pools.Count > 0;
            cmbAddInstrument.DataSource = Enum.GetValues(typeof(CommonData.SupportedInstrument)).Cast<CommonData.SupportedInstrument>().ToArray();
            cmbPoolReward1Diff.DataSource = Enum.GetValues(typeof(CommonData.SupportedDifficulty)).Cast<CommonData.SupportedDifficulty>().ToArray();
            cmbPoolReward2Diff.DataSource = Enum.GetValues(typeof(CommonData.SupportedDifficulty)).Cast<CommonData.SupportedDifficulty>().ToArray();
            cmbPoolReward1Score.DataSource = Enum.GetValues(typeof(APWorldData.CompletionReq)).Cast<APWorldData.CompletionReq>().ToArray();
            cmbPoolReward2Score.DataSource = Enum.GetValues(typeof(APWorldData.CompletionReq)).Cast<APWorldData.CompletionReq>().ToArray();
            UpdateSongReqLabel();
        }

        private bool TryReadSongs(out Dictionary<string, CommonData.SongData> data)
        {
            string Error = "Your available song data was missing or corrupt. Please run the AP build of YARG at least once before launching the client.\n\n" +
                "You may also need to point YARG a valid song path and run a scan for any newly added songs.\nThis can be found in Settings -> Songs in YARG.";
            data = [];
            if (!Directory.Exists(CommonData.DataFolder) || !File.Exists(CommonData.SongExportFile))
            {
                MessageBox.Show(Error, "Song Cache Missing");
                return false;
            }
            try
            {
                CommonData.SongData[]? songData = JsonConvert.DeserializeObject<CommonData.SongData[]>(File.ReadAllText(CommonData.SongExportFile));
                if (songData is null || songData.Length == 0)
                {
                    MessageBox.Show(Error, "Song Cache Corrupt");
                    return false;
                }
                foreach (var d in songData)
                    data.Add(d.SongChecksum, d);
            }
            catch
            {
                MessageBox.Show(Error, "Song Cache Corrupt");
                return false;
            }
            return true;
        }

        #endregion

        #region Event Handlers

        private void SongPoolValueUpdated(object sender, EventArgs e)
        {
            if (PoolUpdating) return;
            if (cmbSelectedPool.SelectedItem is not SongPool Pool) return;
            PoolUpdating = true;

            Pool.MinDifficulty = (int)nudPoolMinDifficulty.Value;
            Pool.MaxDifficulty = (int)nudPoolMaxDifficulty.Value;
            Pool.AmountInPool = (int)nudPoolAmount.Value;
            Pool.CompletionRequirement.Reward1Req = cmbPoolReward1Score.SelectedItem is APWorldData.CompletionReq r1 ? r1 : APWorldData.CompletionReq.Clear;
            Pool.CompletionRequirement.Reward2Req = cmbPoolReward2Score.SelectedItem is APWorldData.CompletionReq r2 ? r2 : APWorldData.CompletionReq.Clear;
            Pool.CompletionRequirement.Reward1Diff = cmbPoolReward1Diff.SelectedItem is CommonData.SupportedDifficulty d1 ? d1 : CommonData.SupportedDifficulty.Expert;
            Pool.CompletionRequirement.Reward2Diff = cmbPoolReward2Diff.SelectedItem is CommonData.SupportedDifficulty d2 ? d2 : CommonData.SupportedDifficulty.Expert;

            var MaxAmount = GetMaxSongsForSongPool(Pool);
            if (Pool.AmountInPool > MaxAmount)
            {
                Pool.AmountInPool = MaxAmount;
                nudPoolAmount.Value = MaxAmount;
                nudPoolAmount.Maximum = MaxAmount;
            }

            PoolUpdating = false;

            lblDisplay.Text = $"Valid songs for [{Pool.Name}]: {Pool.GetAvailableSongs(data.SongData).Count}";
            UpdateSongReqLabel();
        }

        private void cmbSelectedPool_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSelectedPool.SelectedItem is not SongPool SongPool)
            {
                gbCurrentPool.Enabled = false;
                return;
            }

            PoolUpdating = true;

            var MaxAmount = GetMaxSongsForSongPool(SongPool);
            nudPoolAmount.Value = 0;
            nudPoolAmount.Maximum = MaxAmount;

            nudPoolMinDifficulty.Value = SongPool.MinDifficulty;
            nudPoolMaxDifficulty.Value = SongPool.MaxDifficulty;
            nudPoolAmount.Value = SongPool.AmountInPool;
            cmbPoolReward1Diff.SelectedItem = SongPool.CompletionRequirement.Reward1Diff;
            cmbPoolReward2Diff.SelectedItem = SongPool.CompletionRequirement.Reward2Diff;
            cmbPoolReward1Score.SelectedItem = SongPool.CompletionRequirement.Reward1Req;
            cmbPoolReward2Score.SelectedItem = SongPool.CompletionRequirement.Reward2Req;
            gbCurrentPool.Text = $"{SongPool.Name}: ({SongPool.Instrument})";

            PoolUpdating = false;

            lblDisplay.Text = $"Valid songs for [{SongPool.Name}]: {SongPool.GetAvailableSongs(data.SongData).Count}";
            gbCurrentPool.Enabled = true;
        }

        private void btnAddSongPool_Click(object sender, EventArgs e)
        {
            if (cmbAddInstrument.SelectedItem is not SupportedInstrument instrument) return;
            if (string.IsNullOrWhiteSpace(txtAddSongPoolName.Text))
            {
                MessageBox.Show("Invalid Song Pool Name");
                return;
            }
            if (Pools.Any(x => x.Name == txtAddSongPoolName.Text))
            {
                MessageBox.Show("Name already in use");
                return;
            }
            SongPool newSongPool = new(txtAddSongPoolName.Text, instrument);
            Pools.Add(newSongPool);
            txtAddSongPoolName.Text = string.Empty;
            cmbSelectedPool.DataSource = null;
            cmbSelectedPool.DataSource = Pools;
            cmbSelectedPool.SelectedItem = newSongPool;
            gbSongPoolSelect.Enabled = Pools.Count > 0;
        }

        private void btnRemoveSongPool_Click(object sender, EventArgs e)
        {
            if (cmbSelectedPool.SelectedIndex < 0) return;
            Pools.RemoveAt(cmbSelectedPool.SelectedIndex);
            cmbSelectedPool.DataSource = null;
            cmbSelectedPool.DataSource = Pools;
            cmbSelectedPool.SelectedIndex = cmbSelectedPool.Items.Count > 0 ? 0 : -1;
            gbSongPoolSelect.Enabled = Pools.Count > 0;
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            int AddedSongs = Pools.Select(x => x.AmountInPool).Sum();
            int SongsNeeded = data.TotalSongsInPool;
            if (AddedSongs != SongsNeeded)
            {
                MessageBox.Show($"You must add a total of at least {SongsNeeded} songs across all song pools.\n" +
                    $"You have added {AddedSongs} songs across {Pools.Count} song pools.",
                    "Invalid song Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<(SongData Data, SongPool Pool)> PickedSongs = [];
            foreach (var p in Pools)
            {
                Dictionary<string, CommonData.SongData> ValidSongs = p.GetAvailableSongs(data.SongData);
                List<string> availableSongKeys = [.. ValidSongs.Keys];

                for (int i = 0; i < p.AmountInPool; i++)
                {
                    int randomIndex = Connection.GetRNG().Next(availableSongKeys.Count);
                    string chosenKey = availableSongKeys[randomIndex];
                    PickedSongs.Add((ValidSongs[chosenKey], p));
                    availableSongKeys.RemoveAt(randomIndex);
                }
            }
            foreach (var i in data.ApLocationData)
            {
                int randomIndex = Connection.GetRNG().Next(PickedSongs.Count);
                var pickedSong = PickedSongs[randomIndex];
                i.Value.SongHash = pickedSong.Data.SongChecksum;
                i.Value.Requirements = pickedSong.Pool;
                PickedSongs.RemoveAt(randomIndex);
            }
            data.GoalSong.SongHash = PickedSongs[0].Data.SongChecksum;
            data.GoalSong.Requirements = PickedSongs[0].Pool;

            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

        #region Helper Methods

        // Determines the maximum number of songs allowed for a given Song Pool.
        private int GetMaxSongsForSongPool(SongPool Pool)
        {
            var AvailableSongsPerRestriction = Pool.GetAvailableSongs(data.SongData).Count;
            var AmountOfSongsInOtherPools = Pools.Where(x => x.Name != Pool.Name).Select(x => x.AmountInPool).Sum();
            var SongsLeftForThisPool = data.TotalSongsInPool - AmountOfSongsInOtherPools;
            return Math.Min(AvailableSongsPerRestriction, SongsLeftForThisPool);
        }

        private void UpdateSongReqLabel()
        {
            int SelectedSongs = Pools.Select(x => x.AmountInPool).Sum();
            int RequiredSongs = data.TotalSongsInPool;
            lblRequiredSongCount.Text = $"Selected Songs: {SelectedSongs} | Required Songs: {RequiredSongs}";
        }

        #endregion

        private void ConfigForm_Shown(object sender, EventArgs e)
        {
            if (SongError)
                Close();
        }
    }
}
