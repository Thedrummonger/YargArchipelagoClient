

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
            tlpSectionsMain.SuspendLayout();
            tlpConsole.SuspendLayout();
            tlpCheckList.SuspendLayout();
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
            tlpSectionsMain.Location = new Point(0, 0);
            tlpSectionsMain.Name = "tlpSectionsMain";
            tlpSectionsMain.RowCount = 1;
            tlpSectionsMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpSectionsMain.Size = new Size(800, 450);
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
            tlpConsole.Size = new Size(394, 444);
            tlpConsole.TabIndex = 0;
            // 
            // textBox1
            // 
            textBox1.Dock = DockStyle.Fill;
            textBox1.Location = new Point(3, 417);
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
            lbConsole.Size = new Size(388, 408);
            lbConsole.TabIndex = 1;
            lbConsole.Text = "";
            // 
            // btnSendChat
            // 
            btnSendChat.Dock = DockStyle.Fill;
            btnSendChat.Location = new Point(347, 417);
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
            tlpCheckList.Size = new Size(394, 444);
            tlpCheckList.TabIndex = 1;
            // 
            // lvSongList
            // 
            lvSongList.Columns.AddRange(new ColumnHeader[] { columnID, columnSongName });
            lvSongList.Dock = DockStyle.Fill;
            lvSongList.FullRowSelect = true;
            lvSongList.Location = new Point(3, 33);
            lvSongList.Name = "lvSongList";
            lvSongList.Size = new Size(388, 408);
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
            // MainForm
            // 
            AcceptButton = btnSendChat;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tlpSectionsMain);
            Name = "MainForm";
            Text = "Yarg Archipelago Client";
            Shown += MainForm_Shown;
            tlpSectionsMain.ResumeLayout(false);
            tlpConsole.ResumeLayout(false);
            tlpConsole.PerformLayout();
            tlpCheckList.ResumeLayout(false);
            tlpCheckList.PerformLayout();
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
    }
}
