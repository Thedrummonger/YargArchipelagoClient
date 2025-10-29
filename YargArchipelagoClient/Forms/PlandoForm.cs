using System.Diagnostics;
using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Helpers;
using YargArchipelagoCommon;
using YargArchipelagoCore.Data;

namespace YargArchipelagoClient.Forms
{
    public partial class PlandoForm : Form
    {
        ConfigForm Parent;
        public Dictionary<int, PlandoData> PlandoSongData { get; set; }

        SongPoolManager poolManager;

        public PlandoForm(ConfigForm parent, Dictionary<int, PlandoData> Plando, SongPoolManager Manager)
        {
            InitializeComponent();
            Parent = parent;
            PlandoSongData = Plando;
            poolManager = Manager;
        }

        private void PlandoForm_Load(object sender, EventArgs e)
        {
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;
            UpdateLocationList(sender, e);
            cmbAPLocation.SelectedIndex = 0;
        }

        bool Updating = false;

        private void cmbAPLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPoolUIFromSelectedItem();
            LoadSongUIFromSelectedItem();
        }

        private void LoadPoolUIFromSelectedItem()
        {
            if (Updating) return;
            Updating = true;
            var selectedSong = GetSelectedSong();
            if (selectedSong is not null && Parent.Pools.Count > 0 && poolManager.CanPlandoAnyPoolToThisLocation(selectedSong, out var ValidPools))
            {
                groupBox1.Enabled = true;
                chkEnablePoolPlando.Checked = selectedSong.PoolPlandoEnabled;
                cmbPlandoPoolSelect.Enabled = selectedSong.PoolPlandoEnabled;
                cmbPlandoPoolSelect.DataSource = WinFormHelpers.ContainerItem.ToContainerList(ValidPools, x => x.Name, x => x.Name);
                var poolItems = (IEnumerable<WinFormHelpers.ContainerItem>)cmbPlandoPoolSelect.DataSource!;
                var SelectedPoolItem = selectedSong.SongPool is null ? null : poolItems.FirstOrDefault(x => x.Value is string s && s == selectedSong.SongPool);
                cmbPlandoPoolSelect.SelectedItem = SelectedPoolItem is null ? poolItems.FirstOrDefault() : SelectedPoolItem;
            }
            else
            {
                groupBox1.Enabled = false;
            }
            Updating = false;
        }

        private void LoadSongUIFromSelectedItem()
        {
            if (Updating) return;
            Updating = true;
            var selectedSong = GetSelectedSong();
            if (selectedSong is not null && poolManager.GetValidSongsForAllPools().Count > 0 && poolManager.CanPlandoSongToThisLocation(selectedSong))
            {
                groupBox2.Enabled = true;
                chkEnableSongPlando.Checked = selectedSong.SongPlandoEnabled;
                cmbPlandoSongSelect.Enabled = selectedSong.SongPlandoEnabled;
                PrintSongsForPlando(selectedSong);
                var songItems = (IEnumerable<WinFormHelpers.ContainerItem>)cmbPlandoSongSelect.DataSource!;
                var SelectedSongItem = selectedSong.SongHash is null ? null : songItems.FirstOrDefault(x => x.Value is string s && s == selectedSong.SongHash);
                cmbPlandoSongSelect.SelectedItem = SelectedSongItem is null ? songItems.First() : SelectedSongItem;
            }
            else
            {
                groupBox2.Enabled = false;
            }
            Updating = false;
        }

        private void PrintSongsForPlando(PlandoData SelectedSong)
        {
            HashSet<CommonData.SongData> ValidSongs = poolManager.GetValidSongsForAllPools();
            if (SelectedSong.PoolPlandoEnabled)
            {
                var Pool = Parent.Pools.FirstOrDefault(x => x.Name == SelectedSong.SongPool);
                if (Pool is not null)
                    ValidSongs = [.. Pool.GetAvailableSongs(Parent.data.SongData).Values];
            }
            cmbPlandoSongSelect.DataSource = WinFormHelpers.ContainerItem.ToContainerList(ValidSongs, x => x.SongChecksum, x => $"{x.Name} by {x.Artist}").OrderBy(x => x.Display).ToArray();
        }

        private void SongPlandoValueUpdated(object sender, EventArgs e)
        {
            if (Updating) return;
            Updating = true;
            var selectedSong = GetSelectedSong();
            if (selectedSong is not null)
            {
                selectedSong.SongPlandoEnabled = chkEnableSongPlando.Checked;
                selectedSong.SongHash = cmbPlandoSongSelect.GetSelectedContainerItem<string>();
                cmbPlandoSongSelect.Enabled = chkEnableSongPlando.Checked;
            }
            Updating = false;
        }

        private void PoolPlandoValueUpdates(object sender, EventArgs e)
        {
            if (Updating) return;
            Updating = true;
            var selectedSong = GetSelectedSong();
            if (selectedSong is not null)
            {
                selectedSong.PoolPlandoEnabled = chkEnablePoolPlando.Checked;
                selectedSong.SongPool = cmbPlandoPoolSelect.GetSelectedContainerItem<string>();
                cmbPlandoPoolSelect.Enabled = chkEnablePoolPlando.Checked;
            }
            Updating = false;
            LoadSongUIFromSelectedItem();
            SongPlandoValueUpdated(sender, e);
        }

        private PlandoData? GetSelectedSong()
        {
            var selectedSongNum = cmbAPLocation.GetSelectedContainerItem<int?>();
            if (selectedSongNum is null) return null;
            return PlandoSongData[selectedSongNum.Value];
        }

        private void UpdateLocationList(object sender, EventArgs e)
        {
            int[] songNums = [.. PlandoSongData.Keys];
            if (cmbFilterConfigured.CheckState == CheckState.Checked)
                songNums = [.. songNums.Where(x => PlandoSongData[x].HasValidPlando)];
            else if (cmbFilterConfigured.CheckState == CheckState.Indeterminate)
                songNums = [.. songNums.Where(x => !PlandoSongData[x].HasValidPlando)];
            if (txtFilter.Text.Length > 0)
                songNums = [.. songNums.Where(i => poolManager.SongDisplay(i).Contains(txtFilter.Text, StringComparison.CurrentCultureIgnoreCase))];

            cmbAPLocation.DataSource = WinFormHelpers.ContainerItem.ToContainerList(songNums, poolManager.SongDisplay);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            Debug.WriteLine(PlandoSongData.Where(x => x.Value.SongPlandoEnabled || x.Value.PoolPlandoEnabled).ToFormattedJson());
        }
    }
}
