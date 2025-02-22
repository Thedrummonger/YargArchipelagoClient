namespace YargArchipelagoClient
{
    partial class ConfigForm
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
            txtSongPath = new TextBox();
            gbProfile = new GroupBox();
            nudProfileAmount = new NumericUpDown();
            label10 = new Label();
            cmbProfileExtraReqs = new ComboBox();
            cmbProfileReqs = new ComboBox();
            label4 = new Label();
            label5 = new Label();
            nudProfileMaxDifficulty = new NumericUpDown();
            label3 = new Label();
            nudProfileMinDifficulty = new NumericUpDown();
            label2 = new Label();
            btnScanSongs = new Button();
            btnBrowse = new Button();
            lblDisplay = new Label();
            cmbSelectedProfile = new ComboBox();
            cmbAddInstrument = new ComboBox();
            label6 = new Label();
            label7 = new Label();
            txtAddProfileName = new TextBox();
            btnAddProfile = new Button();
            gbAdd = new GroupBox();
            label8 = new Label();
            btnRemoveProfile = new Button();
            btnStartGame = new Button();
            BtnSaveProfile = new Button();
            btnLoadProfile = new Button();
            lblRequiredSongCount = new Label();
            gbProfile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudProfileAmount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudProfileMaxDifficulty).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudProfileMinDifficulty).BeginInit();
            gbAdd.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(58, 15);
            label1.TabIndex = 0;
            label1.Text = "SongPath";
            // 
            // txtSongPath
            // 
            txtSongPath.Location = new Point(12, 27);
            txtSongPath.Name = "txtSongPath";
            txtSongPath.Size = new Size(198, 23);
            txtSongPath.TabIndex = 1;
            // 
            // gbProfile
            // 
            gbProfile.Controls.Add(nudProfileAmount);
            gbProfile.Controls.Add(label10);
            gbProfile.Controls.Add(cmbProfileExtraReqs);
            gbProfile.Controls.Add(cmbProfileReqs);
            gbProfile.Controls.Add(label4);
            gbProfile.Controls.Add(label5);
            gbProfile.Controls.Add(nudProfileMaxDifficulty);
            gbProfile.Controls.Add(label3);
            gbProfile.Controls.Add(nudProfileMinDifficulty);
            gbProfile.Controls.Add(label2);
            gbProfile.Location = new Point(223, 27);
            gbProfile.Name = "gbProfile";
            gbProfile.Size = new Size(200, 206);
            gbProfile.TabIndex = 2;
            gbProfile.TabStop = false;
            gbProfile.Text = "Lead Guitar";
            // 
            // nudProfileAmount
            // 
            nudProfileAmount.Location = new Point(156, 17);
            nudProfileAmount.Name = "nudProfileAmount";
            nudProfileAmount.Size = new Size(38, 23);
            nudProfileAmount.TabIndex = 9;
            nudProfileAmount.ValueChanged += ProfileValueUpdated;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(6, 19);
            label10.Name = "label10";
            label10.Size = new Size(91, 15);
            label10.TabIndex = 8;
            label10.Text = "Amount in Pool";
            // 
            // cmbProfileExtraReqs
            // 
            cmbProfileExtraReqs.FormattingEnabled = true;
            cmbProfileExtraReqs.Location = new Point(6, 171);
            cmbProfileExtraReqs.Name = "cmbProfileExtraReqs";
            cmbProfileExtraReqs.Size = new Size(188, 23);
            cmbProfileExtraReqs.TabIndex = 7;
            cmbProfileExtraReqs.SelectedIndexChanged += ProfileValueUpdated;
            // 
            // cmbProfileReqs
            // 
            cmbProfileReqs.FormattingEnabled = true;
            cmbProfileReqs.Location = new Point(6, 125);
            cmbProfileReqs.Name = "cmbProfileReqs";
            cmbProfileReqs.Size = new Size(188, 23);
            cmbProfileReqs.TabIndex = 5;
            cmbProfileReqs.SelectedIndexChanged += ProfileValueUpdated;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 105);
            label4.Name = "label4";
            label4.Size = new Size(171, 15);
            label4.TabIndex = 4;
            label4.Text = "Song Completion Requirement";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 151);
            label5.Name = "label5";
            label5.Size = new Size(170, 15);
            label5.TabIndex = 6;
            label5.Text = "Extra Completion Requirement";
            // 
            // nudProfileMaxDifficulty
            // 
            nudProfileMaxDifficulty.Location = new Point(156, 75);
            nudProfileMaxDifficulty.Name = "nudProfileMaxDifficulty";
            nudProfileMaxDifficulty.Size = new Size(38, 23);
            nudProfileMaxDifficulty.TabIndex = 3;
            nudProfileMaxDifficulty.ValueChanged += ProfileValueUpdated;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 77);
            label3.Name = "label3";
            label3.Size = new Size(81, 15);
            label3.TabIndex = 2;
            label3.Text = "Max Difficulty";
            // 
            // nudProfileMinDifficulty
            // 
            nudProfileMinDifficulty.Location = new Point(156, 46);
            nudProfileMinDifficulty.Name = "nudProfileMinDifficulty";
            nudProfileMinDifficulty.Size = new Size(38, 23);
            nudProfileMinDifficulty.TabIndex = 1;
            nudProfileMinDifficulty.ValueChanged += ProfileValueUpdated;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 48);
            label2.Name = "label2";
            label2.Size = new Size(79, 15);
            label2.TabIndex = 0;
            label2.Text = "Min Difficulty";
            // 
            // btnScanSongs
            // 
            btnScanSongs.Location = new Point(115, 56);
            btnScanSongs.Name = "btnScanSongs";
            btnScanSongs.Size = new Size(95, 23);
            btnScanSongs.TabIndex = 3;
            btnScanSongs.Text = "Scan Songs";
            btnScanSongs.UseVisualStyleBackColor = true;
            btnScanSongs.Click += btnScanSongs_Click;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(12, 56);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(89, 23);
            btnBrowse.TabIndex = 4;
            btnBrowse.Text = "Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // lblDisplay
            // 
            lblDisplay.AutoSize = true;
            lblDisplay.Location = new Point(223, 9);
            lblDisplay.Name = "lblDisplay";
            lblDisplay.Size = new Size(93, 15);
            lblDisplay.TabIndex = 6;
            lblDisplay.Text = "Available Songs:";
            // 
            // cmbSelectedProfile
            // 
            cmbSelectedProfile.FormattingEnabled = true;
            cmbSelectedProfile.Location = new Point(13, 113);
            cmbSelectedProfile.Name = "cmbSelectedProfile";
            cmbSelectedProfile.Size = new Size(197, 23);
            cmbSelectedProfile.TabIndex = 7;
            cmbSelectedProfile.SelectedIndexChanged += cmbSelectedProfile_SelectedIndexChanged;
            // 
            // cmbAddInstrument
            // 
            cmbAddInstrument.FormattingEnabled = true;
            cmbAddInstrument.Location = new Point(77, 16);
            cmbAddInstrument.Name = "cmbAddInstrument";
            cmbAddInstrument.Size = new Size(117, 23);
            cmbAddInstrument.TabIndex = 8;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(6, 19);
            label6.Name = "label6";
            label6.Size = new Size(65, 15);
            label6.TabIndex = 9;
            label6.Text = "Instrument";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(12, 134);
            label7.Name = "label7";
            label7.Size = new Size(0, 15);
            label7.TabIndex = 10;
            // 
            // txtAddProfileName
            // 
            txtAddProfileName.Location = new Point(6, 46);
            txtAddProfileName.Name = "txtAddProfileName";
            txtAddProfileName.Size = new Size(130, 23);
            txtAddProfileName.TabIndex = 11;
            // 
            // btnAddProfile
            // 
            btnAddProfile.Location = new Point(142, 46);
            btnAddProfile.Name = "btnAddProfile";
            btnAddProfile.Size = new Size(52, 23);
            btnAddProfile.TabIndex = 12;
            btnAddProfile.Text = "Add";
            btnAddProfile.UseVisualStyleBackColor = true;
            btnAddProfile.Click += btnAddProfile_Click;
            // 
            // gbAdd
            // 
            gbAdd.Controls.Add(label6);
            gbAdd.Controls.Add(btnAddProfile);
            gbAdd.Controls.Add(cmbAddInstrument);
            gbAdd.Controls.Add(txtAddProfileName);
            gbAdd.Location = new Point(12, 152);
            gbAdd.Name = "gbAdd";
            gbAdd.Size = new Size(200, 81);
            gbAdd.TabIndex = 13;
            gbAdd.TabStop = false;
            gbAdd.Text = "Add Profile";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(13, 88);
            label8.Name = "label8";
            label8.Size = new Size(88, 15);
            label8.TabIndex = 14;
            label8.Text = "Selected Profile";
            // 
            // btnRemoveProfile
            // 
            btnRemoveProfile.Location = new Point(135, 84);
            btnRemoveProfile.Name = "btnRemoveProfile";
            btnRemoveProfile.Size = new Size(75, 23);
            btnRemoveProfile.TabIndex = 15;
            btnRemoveProfile.Text = "Remove";
            btnRemoveProfile.UseVisualStyleBackColor = true;
            btnRemoveProfile.Click += btnRemoveProfile_Click;
            // 
            // btnStartGame
            // 
            btnStartGame.Location = new Point(13, 237);
            btnStartGame.Name = "btnStartGame";
            btnStartGame.Size = new Size(197, 23);
            btnStartGame.TabIndex = 16;
            btnStartGame.Text = "Start Game!";
            btnStartGame.UseVisualStyleBackColor = true;
            btnStartGame.Click += btnStartGame_Click;
            // 
            // BtnSaveProfile
            // 
            BtnSaveProfile.Location = new Point(223, 237);
            BtnSaveProfile.Name = "BtnSaveProfile";
            BtnSaveProfile.Size = new Size(97, 23);
            BtnSaveProfile.TabIndex = 17;
            BtnSaveProfile.Text = "Save Profile";
            BtnSaveProfile.UseVisualStyleBackColor = true;
            // 
            // btnLoadProfile
            // 
            btnLoadProfile.Location = new Point(326, 237);
            btnLoadProfile.Name = "btnLoadProfile";
            btnLoadProfile.Size = new Size(97, 23);
            btnLoadProfile.TabIndex = 18;
            btnLoadProfile.Text = "Load Profile";
            btnLoadProfile.UseVisualStyleBackColor = true;
            // 
            // lblRequiredSongCount
            // 
            lblRequiredSongCount.AutoSize = true;
            lblRequiredSongCount.Location = new Point(12, 263);
            lblRequiredSongCount.Name = "lblRequiredSongCount";
            lblRequiredSongCount.Size = new Size(57, 15);
            lblRequiredSongCount.TabIndex = 19;
            lblRequiredSongCount.Text = "Required:";
            // 
            // ConfigForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(436, 285);
            Controls.Add(lblRequiredSongCount);
            Controls.Add(btnLoadProfile);
            Controls.Add(BtnSaveProfile);
            Controls.Add(btnStartGame);
            Controls.Add(btnRemoveProfile);
            Controls.Add(label8);
            Controls.Add(gbAdd);
            Controls.Add(label7);
            Controls.Add(cmbSelectedProfile);
            Controls.Add(lblDisplay);
            Controls.Add(btnBrowse);
            Controls.Add(btnScanSongs);
            Controls.Add(gbProfile);
            Controls.Add(txtSongPath);
            Controls.Add(label1);
            Name = "ConfigForm";
            Text = "Configure Game Settings";
            gbProfile.ResumeLayout(false);
            gbProfile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudProfileAmount).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudProfileMaxDifficulty).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudProfileMinDifficulty).EndInit();
            gbAdd.ResumeLayout(false);
            gbAdd.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtSongPath;
        private GroupBox gbProfile;
        private Label label4;
        private NumericUpDown nudProfileMaxDifficulty;
        private Label label3;
        private NumericUpDown nudProfileMinDifficulty;
        private Label label2;
        private ComboBox cmbProfileExtraReqs;
        private ComboBox cmbProfileReqs;
        private Label label5;
        private Button btnScanSongs;
        private Button btnBrowse;
        private NumericUpDown nudProfileAmount;
        private Label label10;
        private Label lblDisplay;
        private ComboBox cmbSelectedProfile;
        private ComboBox cmbAddInstrument;
        private Label label6;
        private Label label7;
        private TextBox txtAddProfileName;
        private Button btnAddProfile;
        private GroupBox gbAdd;
        private Label label8;
        private Button btnRemoveProfile;
        private Button btnStartGame;
        private Button BtnSaveProfile;
        private Button btnLoadProfile;
        private Label lblRequiredSongCount;
    }
}