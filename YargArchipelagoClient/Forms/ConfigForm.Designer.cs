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
            gbCurrentPool = new GroupBox();
            groupBox2 = new GroupBox();
            label4 = new Label();
            label5 = new Label();
            cmbPoolReward2Score = new ComboBox();
            cmbPoolReward2Diff = new ComboBox();
            nudPoolAmount = new NumericUpDown();
            groupBox1 = new GroupBox();
            label13 = new Label();
            label12 = new Label();
            cmbPoolReward1Score = new ComboBox();
            cmbPoolReward1Diff = new ComboBox();
            label10 = new Label();
            nudPoolMaxDifficulty = new NumericUpDown();
            label3 = new Label();
            nudPoolMinDifficulty = new NumericUpDown();
            label2 = new Label();
            lblDisplay = new Label();
            cmbSelectedPool = new ComboBox();
            cmbAddInstrument = new ComboBox();
            label6 = new Label();
            label7 = new Label();
            txtAddSongPoolName = new TextBox();
            btnAddSongPool = new Button();
            gbAdd = new GroupBox();
            btnRemoveSongPool = new Button();
            btnStartGame = new Button();
            BtnSaveSongPoolConfig = new Button();
            btnLoadSongPoolConfig = new Button();
            lblRequiredSongCount = new Label();
            gbSongPoolSelect = new GroupBox();
            button1 = new Button();
            gbCurrentPool.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudPoolAmount).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudPoolMaxDifficulty).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPoolMinDifficulty).BeginInit();
            gbAdd.SuspendLayout();
            gbSongPoolSelect.SuspendLayout();
            SuspendLayout();
            // 
            // gbCurrentPool
            // 
            gbCurrentPool.Controls.Add(groupBox2);
            gbCurrentPool.Controls.Add(nudPoolAmount);
            gbCurrentPool.Controls.Add(groupBox1);
            gbCurrentPool.Controls.Add(label10);
            gbCurrentPool.Controls.Add(nudPoolMaxDifficulty);
            gbCurrentPool.Controls.Add(label3);
            gbCurrentPool.Controls.Add(nudPoolMinDifficulty);
            gbCurrentPool.Controls.Add(label2);
            gbCurrentPool.Location = new Point(238, 27);
            gbCurrentPool.Name = "gbCurrentPool";
            gbCurrentPool.Size = new Size(210, 222);
            gbCurrentPool.TabIndex = 2;
            gbCurrentPool.TabStop = false;
            gbCurrentPool.Text = "Lead Guitar";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(cmbPoolReward2Score);
            groupBox2.Controls.Add(cmbPoolReward2Diff);
            groupBox2.Location = new Point(105, 105);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(93, 111);
            groupBox2.TabIndex = 22;
            groupBox2.TabStop = false;
            groupBox2.Text = "Reward 2";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 63);
            label4.Name = "label4";
            label4.Size = new Size(79, 15);
            label4.TabIndex = 8;
            label4.Text = "Min Difficulty";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(5, 19);
            label5.Name = "label5";
            label5.Size = new Size(60, 15);
            label5.TabIndex = 6;
            label5.Text = "Min Score";
            // 
            // cmbPoolReward2Score
            // 
            cmbPoolReward2Score.FormattingEnabled = true;
            cmbPoolReward2Score.Location = new Point(6, 37);
            cmbPoolReward2Score.Name = "cmbPoolReward2Score";
            cmbPoolReward2Score.Size = new Size(79, 23);
            cmbPoolReward2Score.TabIndex = 5;
            // 
            // cmbPoolReward2Diff
            // 
            cmbPoolReward2Diff.FormattingEnabled = true;
            cmbPoolReward2Diff.Location = new Point(6, 82);
            cmbPoolReward2Diff.Name = "cmbPoolReward2Diff";
            cmbPoolReward2Diff.Size = new Size(79, 23);
            cmbPoolReward2Diff.TabIndex = 7;
            // 
            // nudPoolAmount
            // 
            nudPoolAmount.Location = new Point(160, 17);
            nudPoolAmount.Name = "nudPoolAmount";
            nudPoolAmount.Size = new Size(38, 23);
            nudPoolAmount.TabIndex = 9;
            nudPoolAmount.ValueChanged += SongPoolValueUpdated;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label13);
            groupBox1.Controls.Add(label12);
            groupBox1.Controls.Add(cmbPoolReward1Score);
            groupBox1.Controls.Add(cmbPoolReward1Diff);
            groupBox1.Location = new Point(6, 105);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(93, 111);
            groupBox1.TabIndex = 21;
            groupBox1.TabStop = false;
            groupBox1.Text = "Reward 1";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(6, 63);
            label13.Name = "label13";
            label13.Size = new Size(79, 15);
            label13.TabIndex = 8;
            label13.Text = "Min Difficulty";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(5, 19);
            label12.Name = "label12";
            label12.Size = new Size(60, 15);
            label12.TabIndex = 6;
            label12.Text = "Min Score";
            // 
            // cmbPoolReward1Score
            // 
            cmbPoolReward1Score.FormattingEnabled = true;
            cmbPoolReward1Score.Location = new Point(6, 37);
            cmbPoolReward1Score.Name = "cmbPoolReward1Score";
            cmbPoolReward1Score.Size = new Size(79, 23);
            cmbPoolReward1Score.TabIndex = 5;
            // 
            // cmbPoolReward1Diff
            // 
            cmbPoolReward1Diff.FormattingEnabled = true;
            cmbPoolReward1Diff.Location = new Point(6, 82);
            cmbPoolReward1Diff.Name = "cmbPoolReward1Diff";
            cmbPoolReward1Diff.Size = new Size(79, 23);
            cmbPoolReward1Diff.TabIndex = 7;
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
            // nudPoolMaxDifficulty
            // 
            nudPoolMaxDifficulty.Location = new Point(160, 75);
            nudPoolMaxDifficulty.Name = "nudPoolMaxDifficulty";
            nudPoolMaxDifficulty.Size = new Size(38, 23);
            nudPoolMaxDifficulty.TabIndex = 3;
            nudPoolMaxDifficulty.ValueChanged += SongPoolValueUpdated;
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
            // nudPoolMinDifficulty
            // 
            nudPoolMinDifficulty.Location = new Point(160, 46);
            nudPoolMinDifficulty.Name = "nudPoolMinDifficulty";
            nudPoolMinDifficulty.Size = new Size(38, 23);
            nudPoolMinDifficulty.TabIndex = 1;
            nudPoolMinDifficulty.ValueChanged += SongPoolValueUpdated;
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
            // lblDisplay
            // 
            lblDisplay.AutoSize = true;
            lblDisplay.Location = new Point(242, 9);
            lblDisplay.Name = "lblDisplay";
            lblDisplay.Size = new Size(93, 15);
            lblDisplay.TabIndex = 6;
            lblDisplay.Text = "Available Songs:";
            // 
            // cmbSelectedPool
            // 
            cmbSelectedPool.FormattingEnabled = true;
            cmbSelectedPool.Location = new Point(6, 22);
            cmbSelectedPool.Name = "cmbSelectedPool";
            cmbSelectedPool.Size = new Size(140, 23);
            cmbSelectedPool.TabIndex = 7;
            cmbSelectedPool.SelectedIndexChanged += cmbSelectedPool_SelectedIndexChanged;
            // 
            // cmbAddInstrument
            // 
            cmbAddInstrument.FormattingEnabled = true;
            cmbAddInstrument.Location = new Point(77, 16);
            cmbAddInstrument.Name = "cmbAddInstrument";
            cmbAddInstrument.Size = new Size(133, 23);
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
            // txtAddSongPoolName
            // 
            txtAddSongPoolName.Location = new Point(6, 46);
            txtAddSongPoolName.Name = "txtAddSongPoolName";
            txtAddSongPoolName.Size = new Size(146, 23);
            txtAddSongPoolName.TabIndex = 11;
            // 
            // btnAddSongPool
            // 
            btnAddSongPool.Location = new Point(158, 45);
            btnAddSongPool.Name = "btnAddSongPool";
            btnAddSongPool.Size = new Size(52, 23);
            btnAddSongPool.TabIndex = 12;
            btnAddSongPool.Text = "Add";
            btnAddSongPool.UseVisualStyleBackColor = true;
            btnAddSongPool.Click += btnAddSongPool_Click;
            // 
            // gbAdd
            // 
            gbAdd.Controls.Add(label6);
            gbAdd.Controls.Add(btnAddSongPool);
            gbAdd.Controls.Add(cmbAddInstrument);
            gbAdd.Controls.Add(txtAddSongPoolName);
            gbAdd.Location = new Point(12, 76);
            gbAdd.Name = "gbAdd";
            gbAdd.Size = new Size(216, 81);
            gbAdd.TabIndex = 13;
            gbAdd.TabStop = false;
            gbAdd.Text = "Add Song Pool";
            // 
            // btnRemoveSongPool
            // 
            btnRemoveSongPool.Location = new Point(152, 21);
            btnRemoveSongPool.Name = "btnRemoveSongPool";
            btnRemoveSongPool.Size = new Size(58, 23);
            btnRemoveSongPool.TabIndex = 15;
            btnRemoveSongPool.Text = "Remove";
            btnRemoveSongPool.UseVisualStyleBackColor = true;
            btnRemoveSongPool.Click += btnRemoveSongPool_Click;
            // 
            // btnStartGame
            // 
            btnStartGame.Location = new Point(12, 225);
            btnStartGame.Name = "btnStartGame";
            btnStartGame.Size = new Size(105, 23);
            btnStartGame.TabIndex = 16;
            btnStartGame.Text = "Start Game!";
            btnStartGame.UseVisualStyleBackColor = true;
            btnStartGame.Click += btnStartGame_Click;
            // 
            // BtnSaveSongPoolConfig
            // 
            BtnSaveSongPoolConfig.Font = new Font("Segoe UI", 8F);
            BtnSaveSongPoolConfig.Location = new Point(12, 169);
            BtnSaveSongPoolConfig.Name = "BtnSaveSongPoolConfig";
            BtnSaveSongPoolConfig.Size = new Size(105, 23);
            BtnSaveSongPoolConfig.TabIndex = 17;
            BtnSaveSongPoolConfig.Text = "Save Pool Config";
            BtnSaveSongPoolConfig.UseVisualStyleBackColor = true;
            // 
            // btnLoadSongPoolConfig
            // 
            btnLoadSongPoolConfig.Font = new Font("Segoe UI", 8F);
            btnLoadSongPoolConfig.Location = new Point(123, 169);
            btnLoadSongPoolConfig.Name = "btnLoadSongPoolConfig";
            btnLoadSongPoolConfig.Size = new Size(105, 23);
            btnLoadSongPoolConfig.TabIndex = 18;
            btnLoadSongPoolConfig.Text = "Load Pool Config";
            btnLoadSongPoolConfig.UseVisualStyleBackColor = true;
            // 
            // lblRequiredSongCount
            // 
            lblRequiredSongCount.AutoSize = true;
            lblRequiredSongCount.Location = new Point(12, 204);
            lblRequiredSongCount.Name = "lblRequiredSongCount";
            lblRequiredSongCount.Size = new Size(57, 15);
            lblRequiredSongCount.TabIndex = 19;
            lblRequiredSongCount.Text = "Required:";
            // 
            // gbSongPoolSelect
            // 
            gbSongPoolSelect.Controls.Add(cmbSelectedPool);
            gbSongPoolSelect.Controls.Add(btnRemoveSongPool);
            gbSongPoolSelect.Location = new Point(12, 12);
            gbSongPoolSelect.Name = "gbSongPoolSelect";
            gbSongPoolSelect.Size = new Size(216, 58);
            gbSongPoolSelect.TabIndex = 20;
            gbSongPoolSelect.TabStop = false;
            gbSongPoolSelect.Text = "Selected Song Pool";
            // 
            // button1
            // 
            button1.Location = new Point(123, 226);
            button1.Name = "button1";
            button1.Size = new Size(105, 23);
            button1.TabIndex = 21;
            button1.Text = "Open Plando";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // ConfigForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(460, 261);
            Controls.Add(button1);
            Controls.Add(gbSongPoolSelect);
            Controls.Add(lblRequiredSongCount);
            Controls.Add(btnLoadSongPoolConfig);
            Controls.Add(BtnSaveSongPoolConfig);
            Controls.Add(btnStartGame);
            Controls.Add(gbAdd);
            Controls.Add(label7);
            Controls.Add(lblDisplay);
            Controls.Add(gbCurrentPool);
            Name = "ConfigForm";
            Text = "Configure Game Settings";
            Shown += ConfigForm_Shown;
            gbCurrentPool.ResumeLayout(false);
            gbCurrentPool.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudPoolAmount).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudPoolMaxDifficulty).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPoolMinDifficulty).EndInit();
            gbAdd.ResumeLayout(false);
            gbAdd.PerformLayout();
            gbSongPoolSelect.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private GroupBox gbCurrentPool;
        private NumericUpDown nudPoolMaxDifficulty;
        private Label label3;
        private NumericUpDown nudPoolMinDifficulty;
        private Label label2;
        private NumericUpDown nudPoolAmount;
        private Label label10;
        private Label lblDisplay;
        private ComboBox cmbSelectedPool;
        private ComboBox cmbAddInstrument;
        private Label label6;
        private Label label7;
        private TextBox txtAddSongPoolName;
        private Button btnAddSongPool;
        private GroupBox gbAdd;
        private Button btnRemoveSongPool;
        private Button btnStartGame;
        private Button BtnSaveSongPoolConfig;
        private Button btnLoadSongPoolConfig;
        private Label lblRequiredSongCount;
        private GroupBox groupBox2;
        private Label label4;
        private Label label5;
        private ComboBox cmbPoolReward2Score;
        private ComboBox cmbPoolReward2Diff;
        private GroupBox groupBox1;
        private Label label13;
        private Label label12;
        private ComboBox cmbPoolReward1Score;
        private ComboBox cmbPoolReward1Diff;
        private GroupBox gbSongPoolSelect;
        private Button button1;
    }
}