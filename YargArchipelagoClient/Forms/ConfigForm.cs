using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Forms;
using YargArchipelagoClient.Helpers;
using YargArchipelagoCommon;
using static YargArchipelagoClient.Forms.PlandoForm;
using static YargArchipelagoCommon.CommonData;

namespace YargArchipelagoClient
{
    public partial class ConfigForm : Form
    {
        #region Fields

        public ConfigData data;
        public ConnectionData Connection;
        public List<SongPool> Pools = [];
        public Dictionary<int, PlandoData> PlandoSongData;

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
            PlandoSongData = data.GetSongIndexes().ToDictionary(x => x, x => new PlandoData { SongNum = x });
            if (!SongImporter.TryReadSongs(out var SongData))
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
            }
            nudPoolAmount.Maximum = MaxAmount;

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
            int SongsNeeded = data.TotalSongsInPool - PlandoSongData.Where(x => x.Value.HasPlando).Count();
            if (AddedSongs != SongsNeeded)
            {
                MessageBox.Show($"You must add a total of at least {SongsNeeded} songs across all song pools.\n" +
                    $"You have added {AddedSongs} songs across {Pools.Count} song pools.",
                    "Invalid song Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Dictionary<string, HashSet<string>> UsedSongs = [];

            List<(SongData Data, SongPool Pool)> PickedSongs = [];
            foreach (var p in Pools)
            {
                var ValidSongs = p.GetAvailableSongs(data.SongData);
                List<string> availableSongKeys = [.. ValidSongs.Keys];

                for (int i = 0; i < p.AmountInPool; i++)
                {
                    int randomIndex = Connection.GetRNG().Next(availableSongKeys.Count);
                    string chosenKey = availableSongKeys[randomIndex];
                    PickedSongs.Add((ValidSongs[chosenKey], p));
                    availableSongKeys.RemoveAt(randomIndex);
                    UsedSongs.SetIfEmpty(p.Name, []);
                    UsedSongs[p.Name].Add(chosenKey);
                }
            }
            SongLocation[] SongLocations = [data.GoalSong, .. data.ApLocationData.Values];
            SongLocations = [.. SongLocations.Where(x => !PlandoSongData[x.SongNumber].HasPlando)];

            if (PickedSongs.Count != SongLocations.Length)
                throw new Exception($"Mismatched song pool from Picked Songs {PickedSongs.Count}|{SongLocations.Length}");

            foreach (var i in SongLocations)
            {
                int randomIndex = Connection.GetRNG().Next(PickedSongs.Count);
                var pickedSong = PickedSongs[randomIndex];
                i.SongHash = pickedSong.Data.SongChecksum;
                i.Requirements = pickedSong.Pool;
                PickedSongs.RemoveAt(randomIndex);
            }

            AssignPlandoSongs(UsedSongs);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void AssignPlandoSongs(Dictionary<string, HashSet<string>> UsedSongs)
        {
            var SongToPoolMap = CreateSongToPoolMap();
            int AddedSongs = Pools.Select(x => x.AmountInPool).Sum();
            foreach (var song in PlandoSongData.Values.Where(x => x.HasPlando))
            {
                var HasSongPlando = song.SongPlandoEnabled && song.SongHash is not null && data.SongData.ContainsKey(song.SongHash);
                var HasPoolPlando = song.PoolPlandoEnabled && song.SongPool is not null && Pools.Any(x => x.Name == song.SongPool);

                var SelectedLocation = song.SongNum == 0 ? data.GoalSong : data.ApLocationData[song.SongNum];

                if (HasPoolPlando && HasSongPlando)
                {
                    SelectedLocation.SongHash = song.SongHash;
                    SelectedLocation.Requirements = Pools.First(x => x.Name == song.SongPool);
                }
                else if (HasPoolPlando)
                {
                    var TargetPool = Pools.First(x => x.Name == song.SongPool);
                    SelectedLocation.Requirements = TargetPool;
                    var ValidSongs = TryGetUnusedSong(TargetPool).Values.ToArray();
                    int randomIndex = Connection.GetRNG().Next(ValidSongs.Length);
                    string chosenKey = ValidSongs[randomIndex].SongChecksum;
                    SelectedLocation.SongHash = chosenKey;
                    UsedSongs.SetIfEmpty(TargetPool.Name, []);
                    UsedSongs[TargetPool.Name].Add(chosenKey);
                }
                else if (HasSongPlando)
                {
                    SelectedLocation.SongHash = song.SongHash;
                    var ValidPools = TryGetUnusedPool(song.SongHash!).ToArray();
                    int randomIndex = Connection.GetRNG().Next(ValidPools.Length);
                    string chosenKey = ValidPools[randomIndex];
                    SelectedLocation.Requirements = Pools.First(x => x.Name == chosenKey);
                    UsedSongs.SetIfEmpty(chosenKey, []);
                    UsedSongs[chosenKey].Add(song.SongHash!);
                }
                else
                    throw new Exception($"Song {song.SongNum} was marked to have Plando data but none could be found\n{song.ToFormattedJson()}");
            }

            Dictionary<string, SongData> TryGetUnusedSong(SongPool PoolName)
            {
                var availableSongs = PoolName.GetAvailableSongs(data.SongData);
                var usedForThisPool = UsedSongs.TryGetValue(PoolName.Name, out var us) ? us : [];
                var validSongs = availableSongs.Where(x => !usedForThisPool.Contains(x.Key)).ToList();
                if (validSongs.Count <= 0)
                    validSongs = [.. availableSongs];
                return validSongs.ToDictionary(x => x.Key, x => x.Value);
            }

            string[] TryGetUnusedPool(string SongHash)
            {
                var AllValidPools = SongToPoolMap[SongHash].Select(x => Pools.First(y => y.Name == x));
                var FilteredPools = AllValidPools.Where(x => x.AmountInPool > 0 || AddedSongs == 0); //Try to not use pools that had no entries, unless no pool had entries
                FilteredPools = FilteredPools.Where(x => !UsedSongs.TryGetValue(x.Name, out var s) || !s.Contains(SongHash));
                if (!FilteredPools.Any())
                    FilteredPools = [.. AllValidPools];
                return [.. FilteredPools.Select(x => x.Name)];
            }
        }

        private Dictionary<string, HashSet<string>> CreateSongToPoolMap()
        {
            Dictionary<string, HashSet<string>> SongToPoolMap = [];
            foreach (var p in Pools)
            {
                var AvailableSongs = p.GetAvailableSongs(data.SongData).Values;
                foreach (var i in AvailableSongs)
                {
                    SongToPoolMap.SetIfEmpty(i.SongChecksum, []);
                    SongToPoolMap[i.SongChecksum].Add(p.Name);
                }
            }
            return SongToPoolMap;
        }


        #endregion

        #region Helper Methods

        // Determines the maximum number of songs allowed for a given Song Pool.
        public int GetMaxSongsForSongPool(SongPool Pool)
        {
            var AvailableSongsPerRestriction = Pool.GetAvailableSongs(data.SongData).Count;
            var SongsPlandodToThisPool = PlandoSongData.Values.Where(x => x.PoolPlandoEnabled && x.SongPool == Pool.Name).Count();
            var AmountOfPlaceableSongsForThisPool = AvailableSongsPerRestriction - SongsPlandodToThisPool;

            var AmoutnofSongsPlandod = PlandoSongData.Values.Where(x => (x.PoolPlandoEnabled || x.SongPlandoEnabled)).Count();

            var AmountOfSongsInOtherPools = Pools.Where(x => x.Name != Pool.Name).Select(x => x.AmountInPool).Sum();
            var SongsLeftForThisPool = data.TotalSongsInPool - AmountOfSongsInOtherPools - AmoutnofSongsPlandod;
            return Math.Min(AmountOfPlaceableSongsForThisPool, SongsLeftForThisPool);
        }

        private void UpdateSongReqLabel()
        {
            int SelectedSongs = Pools.Select(x => x.AmountInPool).Sum() + PlandoSongData.Values.Where(x => x.PoolPlandoEnabled || x.SongPlandoEnabled).Count();
            int RequiredSongs = data.TotalSongsInPool;
            lblRequiredSongCount.Text = $"Selected Songs: {SelectedSongs} | Required Songs: {RequiredSongs}";
        }

        #endregion

        private void ConfigForm_Shown(object sender, EventArgs e)
        {
            if (SongError)
                Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var Plando = new PlandoForm(this, PlandoSongData);
            Plando.ShowDialog();
            Debug.WriteLine(PlandoSongData.Where(x => x.Value.SongPlandoEnabled || x.Value.PoolPlandoEnabled).ToFormattedJson());
            cmbSelectedPool_SelectedIndexChanged(sender, e);
            UpdateSongReqLabel();
        }
    }
}
