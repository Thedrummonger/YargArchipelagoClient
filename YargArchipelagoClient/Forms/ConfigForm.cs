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

        public ConfigData data;
        public ConnectionData Connection;
        public readonly List<SongPool> Pools = [];
        public readonly Dictionary<int, PlandoData> PlandoSongData;
        public SongPoolManager SongPoolManager;

        bool PoolUpdating = false;

        bool SongError = false;


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
            SongPoolManager = new(Pools, PlandoSongData, data.TotalAPSongLocations, data.SongData);
            gbCurrentPool.Enabled = false;
            gbSongPoolSelect.Enabled = Pools.Count > 0;
            cmbAddInstrument.DataSource = Enum.GetValues(typeof(CommonData.SupportedInstrument)).Cast<CommonData.SupportedInstrument>().ToArray();
            cmbPoolReward1Diff.DataSource = Enum.GetValues(typeof(CommonData.SupportedDifficulty)).Cast<CommonData.SupportedDifficulty>().ToArray();
            cmbPoolReward2Diff.DataSource = Enum.GetValues(typeof(CommonData.SupportedDifficulty)).Cast<CommonData.SupportedDifficulty>().ToArray();
            cmbPoolReward1Score.DataSource = Enum.GetValues(typeof(APWorldData.CompletionReq)).Cast<APWorldData.CompletionReq>().ToArray();
            cmbPoolReward2Score.DataSource = Enum.GetValues(typeof(APWorldData.CompletionReq)).Cast<APWorldData.CompletionReq>().ToArray();
            UpdateSongReqLabel();
        }

        private void RandomCountToggled(object sender, EventArgs e)
        {
            if (PoolUpdating) return;
            if (cmbSelectedPool.SelectedItem is not SongPool Pool) return;
            Pool.RandomAmount = chkRandomPoolAmount.Checked;
            LoadPoolData(sender, e);
        }

        private void PoolAmountUpdated(object sender, EventArgs e)
        {
            if (PoolUpdating) return;
            if (cmbSelectedPool.SelectedItem is not SongPool Pool) return;
            if (Pool.RandomAmount)
                Pool.RandomWeight = (int)nudPoolAmount.Value;
            else
                Pool.AmountInPool = (int)nudPoolAmount.Value;
            UpdateSongReqLabel();
        }

        private void DifficultyRestrictionUpdated(object sender, EventArgs e)
        {
            if (PoolUpdating) return;
            if (cmbSelectedPool.SelectedItem is not SongPool Pool) return;
            Pool.MaxDifficulty = (int)nudPoolMaxDifficulty.Value;
            Pool.MinDifficulty = (int)nudPoolMinDifficulty.Value;
            LoadPoolData(sender, e);
        }

        private void PoolRequirementsUpdated(object sender, EventArgs e)
        {
            if (PoolUpdating) return;
            if (cmbSelectedPool.SelectedItem is not SongPool Pool) return;
            Pool.CompletionRequirement.Reward1Req = cmbPoolReward1Score.SelectedItem is APWorldData.CompletionReq r1 ? r1 : APWorldData.CompletionReq.Clear;
            Pool.CompletionRequirement.Reward2Req = cmbPoolReward2Score.SelectedItem is APWorldData.CompletionReq r2 ? r2 : APWorldData.CompletionReq.Clear;
            Pool.CompletionRequirement.Reward1Diff = cmbPoolReward1Diff.SelectedItem is CommonData.SupportedDifficulty d1 ? d1 : CommonData.SupportedDifficulty.Expert;
            Pool.CompletionRequirement.Reward2Diff = cmbPoolReward2Diff.SelectedItem is CommonData.SupportedDifficulty d2 ? d2 : CommonData.SupportedDifficulty.Expert;
        }

        private void LoadPoolData(object sender, EventArgs e)
        {
            if (cmbSelectedPool.SelectedItem is not SongPool SongPool)
            {
                gbCurrentPool.Enabled = false;
                return;
            }

            PoolUpdating = true;

            chkRandomPoolAmount.Checked = SongPool.RandomAmount;

            if (SongPool.RandomAmount)
            {
                lblAmountInPool.Text = "Song Pool Weight";
                SongPoolManager.SetNudCurrentWeight(nudPoolAmount, SongPool);
            }
            else
            {
                lblAmountInPool.Text = "Amount in Pool";
                SongPoolManager.SetNudAmountInPool(nudPoolAmount, SongPool);
            }

            nudPoolMinDifficulty.Value = SongPool.MinDifficulty;
            nudPoolMaxDifficulty.Value = SongPool.MaxDifficulty;

            cmbPoolReward1Diff.SelectedItem = SongPool.CompletionRequirement.Reward1Diff;
            cmbPoolReward2Diff.SelectedItem = SongPool.CompletionRequirement.Reward2Diff;
            cmbPoolReward1Score.SelectedItem = SongPool.CompletionRequirement.Reward1Req;
            cmbPoolReward2Score.SelectedItem = SongPool.CompletionRequirement.Reward2Req;

            gbCurrentPool.Text = $"{SongPool.Name}: ({SongPool.Instrument})";

            PoolUpdating = false;

            lblDisplay.Text = $"Valid songs for [{SongPool.Name}]: {SongPoolManager.GetTotalAmountAssignableToThisPoolViaConfig(SongPool)}";
            UpdateSongReqLabel();
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
            int AddedSongs = SongPoolManager.GetOverallAssignedCountForLabel();
            int SongsNeeded = data.TotalAPSongLocations;
            if (SongPoolManager.GetOverallAssignedCountForLabel() != data.TotalAPSongLocations)
            {
                MessageBox.Show($"You must add a total of at least {SongsNeeded} songs across all song pools.\n" +
                    $"You have added {AddedSongs} songs across {Pools.Count} song pools.",
                    "Invalid song Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ClientInitializationHelper.AssignSongs(data, Connection, Pools, PlandoSongData, SongPoolManager);
            DialogResult = DialogResult.OK;
            Close();
        }

        

        private void UpdateSongReqLabel()
        {
            int SelectedSongs = Pools.Select(x => x.AmountInPool).Sum() + PlandoSongData.Values.Where(x => x.PoolPlandoEnabled || x.SongPlandoEnabled).Count();
            int RequiredSongs = data.TotalAPSongLocations;
            lblRequiredSongCount.Text = $"Selected Songs: {SongPoolManager.GetOverallAssignedCountForLabel()} | Required Songs: {RequiredSongs}";
        }


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
            LoadPoolData(sender, e);
            UpdateSongReqLabel();
        }
    }
}
