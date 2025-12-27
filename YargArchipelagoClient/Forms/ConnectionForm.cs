using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using YargArchipelagoCore.Data;
using YargArchipelagoCore.Helpers;
using YargArchipelagoCommon;

namespace YargArchipelagoClient
{
    public partial class ConnectionForm : Form
    {
        #region Constructors and Fields

        public ConnectionData? Connection = null;
        public ConnectionForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Form Event Handlers

        bool TestingMode = false;
        private void ConnectionForm_Shown(object sender, EventArgs e)
        {
            if (File.Exists(CommonData.ConnectionCachePath))
            {
                try
                {
                    var TempConnection = JsonConvert.DeserializeObject<ConnectionData>(File.ReadAllText(CommonData.ConnectionCachePath));
                    txtServerAddress.Text = TempConnection!.Address;
                    txtPassword.Text = TempConnection!.Password;
                    txtSlotName.Text = TempConnection!.SlotName;
                }
                catch { }
            }
        }

        private void btnConnect_click(object sender, EventArgs e)
        {
            var (ip, port) = NetworkHelpers.ParseIpAddress(txtServerAddress.Text);
            if (ip is null)
            {
                MessageBox.Show("Server address was invalid!", "Invalid Connection Details", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ArchipelagoSession session = ArchipelagoSessionFactory.CreateSession(ip, port);
            ConnectionData data = new(txtServerAddress.Text, txtSlotName.Text, txtPassword.Text, session);

            var result = session.TryConnectAndLogin("YAYARG", data.SlotName, ItemsHandlingFlags.AllItems, APWorldData.APVersion, ["AP"], null, data.Password);

            if (result is LoginFailure failure)
            {
                MessageBox.Show($"Failed to connect to server:\n{string.Join("\n", failure.Errors)}");
                return;
            }
            else if (result is not LoginSuccessful)
            {
                MessageBox.Show($"Failed to connect to server: {result.GetType()}");
                return;
            }

            Connection = data;
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion
    }
}
