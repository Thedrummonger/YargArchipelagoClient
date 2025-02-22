
namespace YargArchipelagoClient
{
    partial class ConnectionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            txtServerAddress = new TextBox();
            txtSlotName = new TextBox();
            label2 = new Label();
            txtPassword = new TextBox();
            label3 = new Label();
            btnConnect = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(84, 15);
            label1.TabIndex = 0;
            label1.Text = "Server Address";
            // 
            // txtServerAddress
            // 
            txtServerAddress.Location = new Point(12, 27);
            txtServerAddress.Name = "txtServerAddress";
            txtServerAddress.Size = new Size(198, 23);
            txtServerAddress.TabIndex = 1;
            // 
            // txtSlotName
            // 
            txtSlotName.Location = new Point(12, 71);
            txtSlotName.Name = "txtSlotName";
            txtSlotName.Size = new Size(198, 23);
            txtSlotName.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 53);
            label2.Name = "label2";
            label2.Size = new Size(62, 15);
            label2.TabIndex = 2;
            label2.Text = "Slot Name";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(12, 115);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(198, 23);
            txtPassword.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 97);
            label3.Name = "label3";
            label3.Size = new Size(57, 15);
            label3.TabIndex = 4;
            label3.Text = "Password";
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(12, 144);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(198, 23);
            btnConnect.TabIndex = 9;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_click;
            // 
            // ConnectionForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(224, 180);
            Controls.Add(btnConnect);
            Controls.Add(txtPassword);
            Controls.Add(label3);
            Controls.Add(txtSlotName);
            Controls.Add(label2);
            Controls.Add(txtServerAddress);
            Controls.Add(label1);
            Name = "ConnectionForm";
            Text = "Connect";
            Shown += ConnectionForm_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtServerAddress;
        private TextBox txtSlotName;
        private Label label2;
        private TextBox txtPassword;
        private Label label3;
        private Button btnConnect;
    }
}