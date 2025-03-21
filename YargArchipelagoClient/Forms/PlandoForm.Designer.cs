namespace YargArchipelagoClient.Forms
{
    partial class PlandoForm
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
            cmbAPLocation = new ComboBox();
            label1 = new Label();
            txtFilter = new TextBox();
            label2 = new Label();
            cmbFilterConfigured = new CheckBox();
            groupBox1 = new GroupBox();
            label3 = new Label();
            cmbPlandoPoolSelect = new ComboBox();
            chkEnablePoolPlando = new CheckBox();
            groupBox2 = new GroupBox();
            label4 = new Label();
            cmbPlandoSongSelect = new ComboBox();
            chkEnableSongPlando = new CheckBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // cmbAPLocation
            // 
            cmbAPLocation.FormattingEnabled = true;
            cmbAPLocation.Location = new Point(12, 32);
            cmbAPLocation.Name = "cmbAPLocation";
            cmbAPLocation.Size = new Size(193, 23);
            cmbAPLocation.TabIndex = 0;
            cmbAPLocation.SelectedIndexChanged += cmbAPLocation_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(120, 15);
            label1.TabIndex = 1;
            label1.Text = "Archipelago Location";
            // 
            // txtFilter
            // 
            txtFilter.Location = new Point(12, 76);
            txtFilter.Name = "txtFilter";
            txtFilter.Size = new Size(193, 23);
            txtFilter.TabIndex = 2;
            txtFilter.TextChanged += UpdateLocationList;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 58);
            label2.Name = "label2";
            label2.Size = new Size(36, 15);
            label2.TabIndex = 3;
            label2.Text = "Filter:";
            // 
            // cmbFilterConfigured
            // 
            cmbFilterConfigured.AutoSize = true;
            cmbFilterConfigured.Location = new Point(119, 57);
            cmbFilterConfigured.Name = "cmbFilterConfigured";
            cmbFilterConfigured.Size = new Size(86, 19);
            cmbFilterConfigured.TabIndex = 4;
            cmbFilterConfigured.Text = "Configured";
            cmbFilterConfigured.ThreeState = true;
            cmbFilterConfigured.UseVisualStyleBackColor = true;
            cmbFilterConfigured.CheckedChanged += UpdateLocationList;
            cmbFilterConfigured.TextChanged += UpdateLocationList;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(cmbPlandoPoolSelect);
            groupBox1.Controls.Add(chkEnablePoolPlando);
            groupBox1.Location = new Point(12, 105);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(193, 75);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Song Pool Plando";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 23);
            label3.Name = "label3";
            label3.Size = new Size(31, 15);
            label3.TabIndex = 2;
            label3.Text = "Pool";
            // 
            // cmbPlandoPoolSelect
            // 
            cmbPlandoPoolSelect.FormattingEnabled = true;
            cmbPlandoPoolSelect.Location = new Point(6, 41);
            cmbPlandoPoolSelect.Name = "cmbPlandoPoolSelect";
            cmbPlandoPoolSelect.Size = new Size(181, 23);
            cmbPlandoPoolSelect.TabIndex = 1;
            cmbPlandoPoolSelect.SelectedIndexChanged += PoolPlandoValueUpdates;
            // 
            // chkEnablePoolPlando
            // 
            chkEnablePoolPlando.AutoSize = true;
            chkEnablePoolPlando.Location = new Point(126, 22);
            chkEnablePoolPlando.Name = "chkEnablePoolPlando";
            chkEnablePoolPlando.Size = new Size(61, 19);
            chkEnablePoolPlando.TabIndex = 0;
            chkEnablePoolPlando.Text = "Enable";
            chkEnablePoolPlando.UseVisualStyleBackColor = true;
            chkEnablePoolPlando.CheckStateChanged += PoolPlandoValueUpdates;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(cmbPlandoSongSelect);
            groupBox2.Controls.Add(chkEnableSongPlando);
            groupBox2.Location = new Point(12, 186);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(193, 75);
            groupBox2.TabIndex = 6;
            groupBox2.TabStop = false;
            groupBox2.Text = "Song Plando";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 23);
            label4.Name = "label4";
            label4.Size = new Size(34, 15);
            label4.TabIndex = 2;
            label4.Text = "Song";
            // 
            // cmbPlandoSongSelect
            // 
            cmbPlandoSongSelect.FormattingEnabled = true;
            cmbPlandoSongSelect.Location = new Point(6, 41);
            cmbPlandoSongSelect.Name = "cmbPlandoSongSelect";
            cmbPlandoSongSelect.Size = new Size(181, 23);
            cmbPlandoSongSelect.TabIndex = 1;
            cmbPlandoSongSelect.SelectedIndexChanged += SongPlandoValueUpdated;
            // 
            // chkEnableSongPlando
            // 
            chkEnableSongPlando.AutoSize = true;
            chkEnableSongPlando.Location = new Point(126, 22);
            chkEnableSongPlando.Name = "chkEnableSongPlando";
            chkEnableSongPlando.Size = new Size(61, 19);
            chkEnableSongPlando.TabIndex = 0;
            chkEnableSongPlando.Text = "Enable";
            chkEnableSongPlando.UseVisualStyleBackColor = true;
            chkEnableSongPlando.CheckStateChanged += SongPlandoValueUpdated;
            // 
            // PlandoForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(217, 273);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(cmbFilterConfigured);
            Controls.Add(label2);
            Controls.Add(txtFilter);
            Controls.Add(label1);
            Controls.Add(cmbAPLocation);
            Name = "PlandoForm";
            Text = "Plando";
            Load += PlandoForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cmbAPLocation;
        private Label label1;
        private TextBox txtFilter;
        private Label label2;
        private CheckBox cmbFilterConfigured;
        private GroupBox groupBox1;
        private Label label3;
        private ComboBox cmbPlandoPoolSelect;
        private CheckBox chkEnablePoolPlando;
        private GroupBox groupBox2;
        private Label label4;
        private ComboBox cmbPlandoSongSelect;
        private CheckBox chkEnableSongPlando;
    }
}