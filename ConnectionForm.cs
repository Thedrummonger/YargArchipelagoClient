using Archipelago.MultiClient.Net;
using ArchipelagoPowerTools.Data;
using ArchipelagoPowerTools.Helpers;
using Archipelago.MultiClient.Net.Enums;

namespace YargArchipelagoClient
{
    public partial class ConnectionForm : Form
    {
        #region Constructors and Fields

        public ConnectionData? Connection = null;
        public ConnectionForm() => InitializeComponent();

        #endregion

        #region Form Event Handlers

        bool TestingMode = false;
        private void ConnectionForm_Shown(object sender, EventArgs e)
        {
            if (!TestingMode) return;

            // Auto connect to my test instance
            ArchipelagoSession session = ArchipelagoSessionFactory.CreateSession("localhost");
            Connection = new("localhost", "Player1", "", session);

            var result = session.TryConnectAndLogin("Yarg", "Player1", ItemsHandlingFlags.AllItems, new(0, 5, 1), ["AP", "DeathLink"], null);
            DialogResult = DialogResult.OK;
            Close();
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

            var result = session.TryConnectAndLogin("Yarg", data.SlotName, ItemsHandlingFlags.AllItems, new(0, 5, 1), ["AP", "DeathLink"], null, data.Password);

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
