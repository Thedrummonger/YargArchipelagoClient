using System.ComponentModel;
using TDMUtils;

namespace YargArchipelagoClient.Forms
{
    public partial class APSongMessageBox : Form
    {
        private Label lblMessage = new();
        private FlowLayoutPanel pnlButtons = new();
        public CustomMessageResult Result { get; private set; } = CustomMessageResult.Cancel; // default

        public APSongMessageBox(string message, string title, params CustomMessageResult[] buttons)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(400, 150);

            lblMessage.Text = message;
            lblMessage.AutoSize = true;
            lblMessage.Location = new Point(10, 10);
            Controls.Add(lblMessage);

            pnlButtons.FlowDirection = FlowDirection.RightToLeft;
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Padding = new Padding(10);
            Controls.Add(pnlButtons);

            foreach (var buttonEnum in buttons)
            {
                var btn = new Button { Text = buttonEnum.GetDescription(), AutoSize = true, DialogResult = DialogResult.OK };
                btn.Click += (s, e) => { Result = buttonEnum; Close(); };
                pnlButtons.Controls.Add(btn);
            }
        }

        public static CustomMessageResult Show(string message, string title, params CustomMessageResult[] buttons)
        {
            using var form = new APSongMessageBox(message, title, buttons);
            form.ShowDialog();
            return form.Result;
        }
    }

    public enum CustomMessageResult
    {
        [Description("Reward 1")]
        Reward1 = 1,
        [Description("Reward 2")]
        Reward2 = 2,
        [Description("Both")]
        Both = 3,
        [Description("Cancel")]
        Cancel = 4
    }
}
