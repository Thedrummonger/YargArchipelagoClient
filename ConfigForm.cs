using ArchipelagoPowerTools.Data;
using System.Data;
using YargArchipelagoClient.Data;
using static YargArchipelagoClient.Data.Constants;

namespace YargArchipelagoClient
{
    public partial class ConfigForm : Form
    {
        #region Fields

        public ConfigData data;
        private ConnectionData Connection;
        private List<SongProfile> Profiles = [];

        bool ProfileChanging = false;

        #endregion

        #region Constructor

        public ConfigForm(ConnectionData connection)
        {
            InitializeComponent();
            Connection = connection;
            cmbSelectedProfile.DataSource = Profiles;
            data = new ConfigData();
            data.ParseAPLocations(connection.GetSession());
            gbProfile.Enabled = false;
            gbAdd.Enabled = false;
            cmbAddInstrument.DataSource = Enum.GetValues(typeof(Instrument)).Cast<Instrument>().ToArray();
            cmbProfileReqs.DataSource = Enum.GetValues(typeof(CompletionReq)).Cast<CompletionReq>().ToArray();
            cmbProfileExtraReqs.DataSource = Enum.GetValues(typeof(CompletionReq)).Cast<CompletionReq>().ToArray();
            UpdateSongReqLabel();
        }

        #endregion

        #region Event Handlers

        private void btnScanSongs_Click(object sender, EventArgs e) => ApplySongs(Connection);

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog SongPathSelect = new();
            var R = SongPathSelect.ShowDialog();
            if (R == DialogResult.OK && !string.IsNullOrWhiteSpace(SongPathSelect.SelectedPath))
                txtSongPath.Text = SongPathSelect.SelectedPath;
        }

        private void ProfileValueUpdated(object sender, EventArgs e)
        {
            if (ProfileChanging) return;
            if (cmbSelectedProfile.SelectedItem is not SongProfile profile) return;
            ProfileChanging = true;

            profile.MinDifficulty = (int)nudProfileMinDifficulty.Value;
            profile.MaxDifficulty = (int)nudProfileMaxDifficulty.Value;
            profile.AmountInPool = (int)nudProfileAmount.Value;
            profile.CompletionRequirement = cmbProfileReqs.SelectedIndex < 0 ? CompletionReq.Clear : (CompletionReq)cmbProfileReqs.SelectedIndex;
            profile.ExtraRequirement = cmbProfileExtraReqs.SelectedIndex < 0 ? CompletionReq.Clear : (CompletionReq)cmbProfileExtraReqs.SelectedIndex;

            var MaxAmount = GetMaxSongsForProfile(profile);
            if (profile.AmountInPool > MaxAmount)
            {
                profile.AmountInPool = MaxAmount;
                nudProfileAmount.Value = MaxAmount;
                nudProfileAmount.Maximum = MaxAmount;
            }

            ProfileChanging = false;

            lblDisplay.Text = $"Available songs for Profile [{profile.Name}]: {profile.GetAvailableSongs(data.SongData).Count}";
            UpdateSongReqLabel();
        }

        private void cmbSelectedProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSelectedProfile.SelectedItem is not SongProfile profile)
            {
                gbProfile.Enabled = false;
                return;
            }

            ProfileChanging = true;

            var MaxAmount = GetMaxSongsForProfile(profile);
            nudProfileAmount.Value = 0;
            nudProfileAmount.Maximum = MaxAmount;

            nudProfileMinDifficulty.Value = profile.MinDifficulty;
            nudProfileMaxDifficulty.Value = profile.MaxDifficulty;
            nudProfileAmount.Value = profile.AmountInPool;
            cmbProfileReqs.SelectedItem = profile.CompletionRequirement;
            cmbProfileExtraReqs.SelectedItem = profile.ExtraRequirement;
            gbProfile.Text = $"{profile.Name}: ({profile.instrument})";

            ProfileChanging = false;

            lblDisplay.Text = $"Valid songs for [{profile.Name}]: {profile.GetAvailableSongs(data.SongData).Count}";
            gbProfile.Enabled = true;
        }

        private void btnAddProfile_Click(object sender, EventArgs e)
        {
            if (cmbAddInstrument.SelectedIndex < 0) return;
            if (string.IsNullOrWhiteSpace(txtAddProfileName.Text))
            {
                MessageBox.Show("Invalid Profile Name");
                return;
            }
            if (Profiles.Any(x => x.Name == txtAddProfileName.Text))
            {
                MessageBox.Show("Profile Name already in use");
                return;
            }
            SongProfile newProfile = new(txtAddProfileName.Text, (Instrument)cmbAddInstrument.SelectedIndex);
            Profiles.Add(newProfile);
            txtAddProfileName.Text = string.Empty;
            cmbSelectedProfile.DataSource = null;
            cmbSelectedProfile.DataSource = Profiles;
            cmbSelectedProfile.SelectedItem = newProfile;
        }

        private void btnRemoveProfile_Click(object sender, EventArgs e)
        {
            if (cmbSelectedProfile.SelectedIndex < 0) return;
            Profiles.RemoveAt(cmbSelectedProfile.SelectedIndex);
            cmbSelectedProfile.DataSource = null;
            cmbSelectedProfile.DataSource = Profiles;
            cmbSelectedProfile.SelectedIndex = cmbSelectedProfile.Items.Count > 0 ? 0 : -1;
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            int AddedSongs = Profiles.Select(x => x.AmountInPool).Sum();
            int SongsNeeded = data.TotalSongsInPool;
            if (AddedSongs != SongsNeeded)
            {
                MessageBox.Show($"You must add a total of at least {SongsNeeded} songs across all profiles.\n" +
                    $"You have added {AddedSongs} songs across {Profiles.Count} Profiles.",
                    "Invalid song Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string seed = Connection.GetSession()!.RoomState.Seed;
            int seedValue = seed.GetHashCode();
            Random rng = new(seedValue);
            List<(string Name, SongData Data, SongProfile profile)> PickedSongs = [];
            foreach (var p in Profiles)
            {
                Dictionary<string, SongData> ValidSongs = p.GetAvailableSongs(data.SongData);
                List<string> availableSongKeys = [.. ValidSongs.Keys];

                for (int i = 0; i < p.AmountInPool; i++)
                {
                    int randomIndex = rng.Next(availableSongKeys.Count);
                    string chosenKey = availableSongKeys[randomIndex];
                    PickedSongs.Add((chosenKey, ValidSongs[chosenKey], p));
                    availableSongKeys.RemoveAt(randomIndex);
                }
            }
            foreach (var i in data.ApLocationData)
            {
                int randomIndex = rng.Next(PickedSongs.Count);
                var pickedSong = PickedSongs[randomIndex];
                i.Value.MappedSong = pickedSong.Name;
                i.Value.Requirements = pickedSong.profile;
                PickedSongs.RemoveAt(randomIndex);
            }
            data.GoalSong.MappedSong = PickedSongs[0].Name;
            data.GoalSong.Requirements = PickedSongs[0].profile;

            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

        #region Helper Methods

        private void ApplySongs(ConnectionData connection)
        {
            string SongFolder = txtSongPath.Text;
            if (!Directory.Exists(SongFolder))
            {
                MessageBox.Show($"Song folder invalid: {SongFolder}");
                return;
            }
            data.SongData = SongLoader.LoadSongs(SongFolder);
            gbAdd.Enabled = data.SongData.Count > 0;
        }

        // Determines the maximum number of songs allowed for a given profile.
        private int GetMaxSongsForProfile(SongProfile profile)
        {
            var AvailableSongsPerRestriction = profile.GetAvailableSongs(data.SongData).Count;
            var AmountOfSongsInOtherProfiles = Profiles.Where(x => x.Name != profile.Name).Select(x => x.AmountInPool).Sum();
            var SongsLeftForThisProfile = data.TotalSongsInPool - AmountOfSongsInOtherProfiles;
            return Math.Min(AvailableSongsPerRestriction, SongsLeftForThisProfile);
        }

        private void UpdateSongReqLabel()
        {
            int SelectedSongs = Profiles.Select(x => x.AmountInPool).Sum();
            int RequiredSongs = data.TotalSongsInPool;
            lblRequiredSongCount.Text = $"Selected Songs: {SelectedSongs} | Required Songs: {RequiredSongs} | ({SelectedSongs}\\{RequiredSongs})";
        }

        #endregion
    }
}
