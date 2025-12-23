

namespace YargArchipelagoClient
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tlpSectionsMain = new TableLayoutPanel();
            tlpConsole = new TableLayoutPanel();
            textBox1 = new TextBox();
            lbConsole = new RichTextBox();
            btnSendChat = new Button();
            tlpCheckList = new TableLayoutPanel();
            lvSongList = new ListView();
            columnID = new ColumnHeader();
            columnSongName = new ColumnHeader();
            txtFilter = new TextBox();
            menuStrip1 = new MenuStrip();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            broadcastSongNamesToolStripMenuItem = new ToolStripMenuItem();
            manualModeToolStripMenuItem = new ToolStripMenuItem();
            deathLinkToolStripMenuItem = new ToolStripMenuItem();
            yARGNotificationsToolStripMenuItem = new ToolStripMenuItem();
            cmbItemNotifMode = new ToolStripComboBox();
            yARGChatNotificationsToolStripMenuItem = new ToolStripMenuItem();
            utilityToolStripMenuItem = new ToolStripMenuItem();
            updateAvailableSongsToolStripMenuItem = new ToolStripMenuItem();
            rescanSongListToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            aPServerToolStripMenuItem = new ToolStripMenuItem();
            changeServerToolStripMenuItem = new ToolStripMenuItem();
            fame0ToolStripMenuItem = new ToolStripMenuItem();
            yARGConnectedToolStripMenuItem = new ToolStripMenuItem();
            currentSongToolStripMenuItem = new ToolStripMenuItem();
            tlpSectionsMain.SuspendLayout();
            tlpConsole.SuspendLayout();
            tlpCheckList.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tlpSectionsMain
            // 
            tlpSectionsMain.AutoSize = true;
            tlpSectionsMain.ColumnCount = 2;
            tlpSectionsMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpSectionsMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpSectionsMain.Controls.Add(tlpConsole, 1, 0);
            tlpSectionsMain.Controls.Add(tlpCheckList, 0, 0);
            tlpSectionsMain.Dock = DockStyle.Fill;
            tlpSectionsMain.Location = new Point(0, 24);
            tlpSectionsMain.Name = "tlpSectionsMain";
            tlpSectionsMain.RowCount = 1;
            tlpSectionsMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpSectionsMain.Size = new Size(800, 426);
            tlpSectionsMain.TabIndex = 0;
            // 
            // tlpConsole
            // 
            tlpConsole.ColumnCount = 2;
            tlpConsole.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpConsole.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            tlpConsole.Controls.Add(textBox1, 0, 1);
            tlpConsole.Controls.Add(lbConsole, 0, 0);
            tlpConsole.Controls.Add(btnSendChat, 1, 1);
            tlpConsole.Dock = DockStyle.Fill;
            tlpConsole.Location = new Point(403, 3);
            tlpConsole.Name = "tlpConsole";
            tlpConsole.RowCount = 2;
            tlpConsole.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpConsole.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpConsole.Size = new Size(394, 420);
            tlpConsole.TabIndex = 0;
            // 
            // textBox1
            // 
            textBox1.Dock = DockStyle.Fill;
            textBox1.Location = new Point(3, 393);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(338, 23);
            textBox1.TabIndex = 0;
            // 
            // lbConsole
            // 
            lbConsole.BackColor = Color.FromArgb(48, 48, 48);
            tlpConsole.SetColumnSpan(lbConsole, 2);
            lbConsole.Dock = DockStyle.Fill;
            lbConsole.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbConsole.ForeColor = Color.White;
            lbConsole.Location = new Point(3, 3);
            lbConsole.Name = "lbConsole";
            lbConsole.ReadOnly = true;
            lbConsole.Size = new Size(388, 384);
            lbConsole.TabIndex = 1;
            lbConsole.Text = "";
            // 
            // btnSendChat
            // 
            btnSendChat.Dock = DockStyle.Fill;
            btnSendChat.Location = new Point(347, 393);
            btnSendChat.Name = "btnSendChat";
            btnSendChat.Size = new Size(44, 24);
            btnSendChat.TabIndex = 2;
            btnSendChat.Text = "Send";
            btnSendChat.UseVisualStyleBackColor = true;
            btnSendChat.Click += btnSendChat_Click;
            // 
            // tlpCheckList
            // 
            tlpCheckList.ColumnCount = 1;
            tlpCheckList.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tlpCheckList.Controls.Add(lvSongList, 0, 1);
            tlpCheckList.Controls.Add(txtFilter, 0, 0);
            tlpCheckList.Dock = DockStyle.Fill;
            tlpCheckList.Location = new Point(3, 3);
            tlpCheckList.Name = "tlpCheckList";
            tlpCheckList.RowCount = 2;
            tlpCheckList.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlpCheckList.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpCheckList.Size = new Size(394, 420);
            tlpCheckList.TabIndex = 1;
            // 
            // lvSongList
            // 
            lvSongList.Columns.AddRange(new ColumnHeader[] { columnID, columnSongName });
            lvSongList.Dock = DockStyle.Fill;
            lvSongList.FullRowSelect = true;
            lvSongList.Location = new Point(3, 33);
            lvSongList.Name = "lvSongList";
            lvSongList.Size = new Size(388, 384);
            lvSongList.TabIndex = 0;
            lvSongList.UseCompatibleStateImageBehavior = false;
            lvSongList.View = View.Details;
            lvSongList.DoubleClick += lvSongList_DoubleClick;
            lvSongList.MouseUp += lvSongList_MouseUp;
            lvSongList.Resize += lvSongList_Resize;
            // 
            // columnID
            // 
            columnID.Text = "ID";
            columnID.Width = 30;
            // 
            // columnSongName
            // 
            columnSongName.Text = "Song Name";
            // 
            // txtFilter
            // 
            txtFilter.Dock = DockStyle.Fill;
            txtFilter.Location = new Point(3, 3);
            txtFilter.Name = "txtFilter";
            txtFilter.Size = new Size(388, 23);
            txtFilter.TabIndex = 1;
            txtFilter.TextChanged += txtFilter_TextChanged;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { settingsToolStripMenuItem, utilityToolStripMenuItem, fame0ToolStripMenuItem, yARGConnectedToolStripMenuItem, currentSongToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { broadcastSongNamesToolStripMenuItem, manualModeToolStripMenuItem, deathLinkToolStripMenuItem, yARGNotificationsToolStripMenuItem, yARGChatNotificationsToolStripMenuItem });
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(61, 20);
            settingsToolStripMenuItem.Text = "Settings";
            settingsToolStripMenuItem.DropDownOpening += UpdatedDropDownChecks;
            // 
            // broadcastSongNamesToolStripMenuItem
            // 
            broadcastSongNamesToolStripMenuItem.Name = "broadcastSongNamesToolStripMenuItem";
            broadcastSongNamesToolStripMenuItem.Size = new Size(202, 22);
            broadcastSongNamesToolStripMenuItem.Text = "Broadcast Song Names";
            broadcastSongNamesToolStripMenuItem.Visible = false;
            broadcastSongNamesToolStripMenuItem.Click += broadcastSongNamesToolStripMenuItem_Click;
            // 
            // manualModeToolStripMenuItem
            // 
            manualModeToolStripMenuItem.Name = "manualModeToolStripMenuItem";
            manualModeToolStripMenuItem.Size = new Size(202, 22);
            manualModeToolStripMenuItem.Text = "Manual Mode";
            manualModeToolStripMenuItem.Visible = false;
            manualModeToolStripMenuItem.Click += manualModeToolStripMenuItem_Click;
            // 
            // deathLinkToolStripMenuItem
            // 
            deathLinkToolStripMenuItem.Name = "deathLinkToolStripMenuItem";
            deathLinkToolStripMenuItem.Size = new Size(202, 22);
            deathLinkToolStripMenuItem.Text = "DeathLink";
            deathLinkToolStripMenuItem.Click += deathLinkToolStripMenuItem_Click;
            // 
            // yARGNotificationsToolStripMenuItem
            // 
            yARGNotificationsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { cmbItemNotifMode });
            yARGNotificationsToolStripMenuItem.Name = "yARGNotificationsToolStripMenuItem";
            yARGNotificationsToolStripMenuItem.Size = new Size(202, 22);
            yARGNotificationsToolStripMenuItem.Text = "YARG Item Notifications";
            // 
            // cmbItemNotifMode
            // 
            cmbItemNotifMode.Items.AddRange(new object[] { "None", "My Items", "All Items" });
            cmbItemNotifMode.Name = "cmbItemNotifMode";
            cmbItemNotifMode.Size = new Size(121, 23);
            cmbItemNotifMode.SelectedIndexChanged += cmbItemNotifMode_SelectedIndexChanged;
            // 
            // yARGChatNotificationsToolStripMenuItem
            // 
            yARGChatNotificationsToolStripMenuItem.Name = "yARGChatNotificationsToolStripMenuItem";
            yARGChatNotificationsToolStripMenuItem.Size = new Size(202, 22);
            yARGChatNotificationsToolStripMenuItem.Text = "YARG Chat Notifications";
            yARGChatNotificationsToolStripMenuItem.Click += yARGChatNotificationsToolStripMenuItem_Click;
            // 
            // utilityToolStripMenuItem
            // 
            utilityToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { updateAvailableSongsToolStripMenuItem, rescanSongListToolStripMenuItem, toolStripSeparator1, aPServerToolStripMenuItem, changeServerToolStripMenuItem });
            utilityToolStripMenuItem.Name = "utilityToolStripMenuItem";
            utilityToolStripMenuItem.Size = new Size(50, 20);
            utilityToolStripMenuItem.Text = "Utility";
            // 
            // updateAvailableSongsToolStripMenuItem
            // 
            updateAvailableSongsToolStripMenuItem.Name = "updateAvailableSongsToolStripMenuItem";
            updateAvailableSongsToolStripMenuItem.Size = new Size(180, 22);
            updateAvailableSongsToolStripMenuItem.Text = "Sync with YARG";
            updateAvailableSongsToolStripMenuItem.Click += updateAvailableSongsToolStripMenuItem_Click;
            // 
            // rescanSongListToolStripMenuItem
            // 
            rescanSongListToolStripMenuItem.Name = "rescanSongListToolStripMenuItem";
            rescanSongListToolStripMenuItem.Size = new Size(180, 22);
            rescanSongListToolStripMenuItem.Text = "Rescan Song List";
            rescanSongListToolStripMenuItem.Click += rescanSongListToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(177, 6);
            // 
            // aPServerToolStripMenuItem
            // 
            aPServerToolStripMenuItem.Enabled = false;
            aPServerToolStripMenuItem.Name = "aPServerToolStripMenuItem";
            aPServerToolStripMenuItem.Size = new Size(180, 22);
            aPServerToolStripMenuItem.Text = "AP Server:";
            // 
            // changeServerToolStripMenuItem
            // 
            changeServerToolStripMenuItem.Name = "changeServerToolStripMenuItem";
            changeServerToolStripMenuItem.Size = new Size(180, 22);
            changeServerToolStripMenuItem.Text = "Disconnect from AP";
            changeServerToolStripMenuItem.Click += ResetConnection;
            // 
            // fame0ToolStripMenuItem
            // 
            fame0ToolStripMenuItem.Name = "fame0ToolStripMenuItem";
            fame0ToolStripMenuItem.Size = new Size(60, 20);
            fame0ToolStripMenuItem.Text = "Fame: 0";
            // 
            // yARGConnectedToolStripMenuItem
            // 
            yARGConnectedToolStripMenuItem.Name = "yARGConnectedToolStripMenuItem";
            yARGConnectedToolStripMenuItem.Size = new Size(109, 20);
            yARGConnectedToolStripMenuItem.Text = "YARG Connected";
            // 
            // currentSongToolStripMenuItem
            // 
            currentSongToolStripMenuItem.Name = "currentSongToolStripMenuItem";
            currentSongToolStripMenuItem.Size = new Size(92, 20);
            currentSongToolStripMenuItem.Text = "Current Song:";
            // 
            // MainForm
            // 
            AcceptButton = btnSendChat;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tlpSectionsMain);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            Text = "Yarg Archipelago Client";
            Shown += MainForm_Shown;
            tlpSectionsMain.ResumeLayout(false);
            tlpConsole.ResumeLayout(false);
            tlpConsole.PerformLayout();
            tlpCheckList.ResumeLayout(false);
            tlpCheckList.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TableLayoutPanel tlpSectionsMain;
        private TableLayoutPanel tlpConsole;
        private TextBox textBox1;
        private TableLayoutPanel tlpCheckList;
        private Button btnSendChat;
        private ListView lvSongList;
        private ColumnHeader columnID;
        private ColumnHeader columnSongName;
        private TextBox txtFilter;
        private RichTextBox lbConsole;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem broadcastSongNamesToolStripMenuItem;
        private ToolStripMenuItem manualModeToolStripMenuItem;
        private ToolStripMenuItem deathLinkToolStripMenuItem;
        private ToolStripMenuItem fame0ToolStripMenuItem;
        private ToolStripMenuItem utilityToolStripMenuItem;
        private ToolStripMenuItem updateAvailableSongsToolStripMenuItem;
        private ToolStripMenuItem rescanSongListToolStripMenuItem;
        private ToolStripMenuItem yARGNotificationsToolStripMenuItem;
        private ToolStripComboBox cmbItemNotifMode;
        private ToolStripMenuItem yARGChatNotificationsToolStripMenuItem;
        private ToolStripMenuItem yARGConnectedToolStripMenuItem;
        private ToolStripMenuItem currentSongToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem changeServerToolStripMenuItem;
        private ToolStripMenuItem aPServerToolStripMenuItem;
    }
}
