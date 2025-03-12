namespace YargArchipelagoClient.Forms
{
    public partial class ValueSelectForm : Form
    {
        public ValueSelectForm()
        {
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
        }
        public object? SelectedValue { get; private set; }

        private ListBox listBox = new() { Dock = DockStyle.Fill };
        private Button okButton = new() { Text = "OK", Dock = DockStyle.Bottom };
        private Button cancelButton = new() { Text = "Cancel", Dock = DockStyle.Bottom };

        public ValueSelectForm(object[] values, string title = "Select a Value")
        {
            Text = title;
            Width = 300;
            Height = 400;
            StartPosition = FormStartPosition.CenterParent;
            okButton.Enabled = false;

            listBox.Items.AddRange(values);

            listBox.SelectedIndexChanged += (s, e) => { okButton.Enabled = listBox.SelectedItem is not null; };
            listBox.DoubleClick += SelectItem;
            okButton.Click += SelectItem;
            cancelButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };

            okButton.Width = cancelButton.Width = (Width - 30) / 2;
            okButton.Left = 10;
            cancelButton.Left = okButton.Right + 10;
            okButton.Top = cancelButton.Top = 5;
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);

            Controls.Add(listBox);
            Controls.Add(buttonPanel);
        }

        private void SelectItem(object? sender, EventArgs e)
        {
            if (listBox.SelectedItem is null) return;
            SelectedValue = listBox.SelectedItem;
            DialogResult = DialogResult.OK;
            Close();
        }

        public static object? ShowDialog(IEnumerable<object> values, string title = "Select a Value")
        {
            using var form = new ValueSelectForm([.. values], title);
            return form.ShowDialog() == DialogResult.OK ? form.SelectedValue : null;
        }
    }
}
