namespace YargArchipelagoClient.Forms
{
    partial class EnergyShop
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
            lblCurrentEnergy = new Label();
            btnPurchaseSwapRand = new Button();
            btnPurchaseSwapPick = new Button();
            btnPurchaseLowerDif = new Button();
            lblAvailableSSR = new Label();
            lblAvailableSSP = new Label();
            lblAvailableLD = new Label();
            lblCurEnergyVal = new Label();
            lblSwapSongRandPrice = new Label();
            lblSwapSongPickPrice = new Label();
            lblLowerDiffPrice = new Label();
            SuspendLayout();
            // 
            // lblCurrentEnergy
            // 
            lblCurrentEnergy.AutoSize = true;
            lblCurrentEnergy.Location = new Point(12, 9);
            lblCurrentEnergy.Name = "lblCurrentEnergy";
            lblCurrentEnergy.Size = new Size(89, 15);
            lblCurrentEnergy.TabIndex = 0;
            lblCurrentEnergy.Text = "Current Energy:";
            // 
            // btnPurchaseSwapRand
            // 
            btnPurchaseSwapRand.Location = new Point(12, 42);
            btnPurchaseSwapRand.Name = "btnPurchaseSwapRand";
            btnPurchaseSwapRand.Size = new Size(185, 23);
            btnPurchaseSwapRand.TabIndex = 1;
            btnPurchaseSwapRand.Text = "Purchase Swap Song (Random)";
            btnPurchaseSwapRand.UseVisualStyleBackColor = true;
            btnPurchaseSwapRand.Click += btnPurchaseSwapRand_Click;
            // 
            // btnPurchaseSwapPick
            // 
            btnPurchaseSwapPick.Location = new Point(12, 94);
            btnPurchaseSwapPick.Name = "btnPurchaseSwapPick";
            btnPurchaseSwapPick.Size = new Size(185, 23);
            btnPurchaseSwapPick.TabIndex = 2;
            btnPurchaseSwapPick.Text = "Purchase Swap Song (Pick)";
            btnPurchaseSwapPick.UseVisualStyleBackColor = true;
            btnPurchaseSwapPick.Click += btnPurchaseSwapPick_Click;
            // 
            // btnPurchaseLowerDif
            // 
            btnPurchaseLowerDif.Location = new Point(12, 146);
            btnPurchaseLowerDif.Name = "btnPurchaseLowerDif";
            btnPurchaseLowerDif.Size = new Size(185, 23);
            btnPurchaseLowerDif.TabIndex = 3;
            btnPurchaseLowerDif.Text = "Purchase Lower Difficulty";
            btnPurchaseLowerDif.UseVisualStyleBackColor = true;
            btnPurchaseLowerDif.Click += btnPurchaseLowerDif_Click;
            // 
            // lblAvailableSSR
            // 
            lblAvailableSSR.AutoSize = true;
            lblAvailableSSR.Location = new Point(122, 68);
            lblAvailableSSR.Name = "lblAvailableSSR";
            lblAvailableSSR.Size = new Size(58, 15);
            lblAvailableSSR.TabIndex = 4;
            lblAvailableSSR.Text = "Available:";
            // 
            // lblAvailableSSP
            // 
            lblAvailableSSP.AutoSize = true;
            lblAvailableSSP.Location = new Point(122, 120);
            lblAvailableSSP.Name = "lblAvailableSSP";
            lblAvailableSSP.Size = new Size(58, 15);
            lblAvailableSSP.TabIndex = 6;
            lblAvailableSSP.Text = "Available:";
            // 
            // lblAvailableLD
            // 
            lblAvailableLD.AutoSize = true;
            lblAvailableLD.Location = new Point(122, 175);
            lblAvailableLD.Name = "lblAvailableLD";
            lblAvailableLD.Size = new Size(58, 15);
            lblAvailableLD.TabIndex = 8;
            lblAvailableLD.Text = "Available:";
            // 
            // lblCurEnergyVal
            // 
            lblCurEnergyVal.AutoSize = true;
            lblCurEnergyVal.Location = new Point(12, 24);
            lblCurEnergyVal.Name = "lblCurEnergyVal";
            lblCurEnergyVal.Size = new Size(13, 15);
            lblCurEnergyVal.TabIndex = 10;
            lblCurEnergyVal.Text = "0";
            // 
            // lblSwapSongRandPrice
            // 
            lblSwapSongRandPrice.AutoSize = true;
            lblSwapSongRandPrice.Location = new Point(12, 68);
            lblSwapSongRandPrice.Name = "lblSwapSongRandPrice";
            lblSwapSongRandPrice.Size = new Size(87, 15);
            lblSwapSongRandPrice.TabIndex = 11;
            lblSwapSongRandPrice.Text = "Price: 17 Billion";
            // 
            // lblSwapSongPickPrice
            // 
            lblSwapSongPickPrice.AutoSize = true;
            lblSwapSongPickPrice.Location = new Point(12, 120);
            lblSwapSongPickPrice.Name = "lblSwapSongPickPrice";
            lblSwapSongPickPrice.Size = new Size(87, 15);
            lblSwapSongPickPrice.TabIndex = 12;
            lblSwapSongPickPrice.Text = "Price: 20 Billion";
            // 
            // lblLowerDiffPrice
            // 
            lblLowerDiffPrice.AutoSize = true;
            lblLowerDiffPrice.Location = new Point(12, 175);
            lblLowerDiffPrice.Name = "lblLowerDiffPrice";
            lblLowerDiffPrice.Size = new Size(87, 15);
            lblLowerDiffPrice.TabIndex = 13;
            lblLowerDiffPrice.Text = "Price: 15 Billion";
            // 
            // EnergyShop
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(211, 207);
            Controls.Add(lblLowerDiffPrice);
            Controls.Add(lblSwapSongPickPrice);
            Controls.Add(lblSwapSongRandPrice);
            Controls.Add(lblCurEnergyVal);
            Controls.Add(lblAvailableLD);
            Controls.Add(lblAvailableSSP);
            Controls.Add(lblAvailableSSR);
            Controls.Add(btnPurchaseLowerDif);
            Controls.Add(btnPurchaseSwapPick);
            Controls.Add(btnPurchaseSwapRand);
            Controls.Add(lblCurrentEnergy);
            Name = "EnergyShop";
            Text = "Shop";
            FormClosing += EnergyShop_FormClosing;
            Load += EnergyShop_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblCurrentEnergy;
        private Button btnPurchaseSwapRand;
        private Button btnPurchaseSwapPick;
        private Button btnPurchaseLowerDif;
        private Label lblAvailableSSR;
        private Label lblAvailableSSP;
        private Label lblAvailableLD;
        private Label lblCurEnergyVal;
        private Label lblSwapSongRandPrice;
        private Label lblSwapSongPickPrice;
        private Label lblLowerDiffPrice;
    }
}