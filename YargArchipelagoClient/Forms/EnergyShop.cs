using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YargArchipelagoCore.Helpers;
using YargArchipelagoCore.Data;

namespace YargArchipelagoClient.Forms
{
    public partial class EnergyShop : Form
    {
        private new readonly MainForm ParentForm;
        private readonly ConnectionData connection;
        private readonly ConfigData config;
        public EnergyShop(MainForm Parent)
        {
            InitializeComponent();
            ParentForm = Parent;
            lblLowerDiffPrice.Text = $"Price: {ExtraAPFunctionalityHelper.FormatLargeNumber(ExtraAPFunctionalityHelper.LowerDifficultyPrice)}";
            lblSwapSongPickPrice.Text = $"Price: {ExtraAPFunctionalityHelper.FormatLargeNumber(ExtraAPFunctionalityHelper.SwapSongPickPrice)}";
            lblSwapSongRandPrice.Text = $"Price: {ExtraAPFunctionalityHelper.FormatLargeNumber(ExtraAPFunctionalityHelper.SwapSongRandomPrice)}";
            connection = ParentForm.Connection;
            config = ParentForm.Config;
            ParentForm.energyShop = this;
        }

        private void EnergyShop_Load(object sender, EventArgs e)
        {
            connection.clientSyncHelper.ConstantCallback += ClientSyncHelper_ConstantCallback;
        }
        private void EnergyShop_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.clientSyncHelper.ConstantCallback -= ClientSyncHelper_ConstantCallback;
            ParentForm.energyShop = null;
        }
        private void ClientSyncHelper_ConstantCallback()
        {
            if (InvokeRequired)
                BeginInvoke(ClientSyncHelper_ConstantCallback);
            else
            {
                long Energy = ExtraAPFunctionalityHelper.GetEnergy(connection, config);
                HashSet<APWorldData.StaticYargAPItem> AvailableItems = [.. connection!.ApItemsRecieved, .. config!.ApItemsPurchased];
                lblCurEnergyVal.Text = Energy.ToString("N0");

                btnPurchaseLowerDif.Enabled = Energy >= ExtraAPFunctionalityHelper.LowerDifficultyPrice;
                btnPurchaseSwapPick.Enabled = Energy >= ExtraAPFunctionalityHelper.SwapSongPickPrice;
                btnPurchaseSwapRand.Enabled = Energy >= ExtraAPFunctionalityHelper.SwapSongRandomPrice;

                AvailableItems = [.. AvailableItems.Where(x => !config.ApItemsUsed.Contains(x))];
                lblAvailableSSR.Text = "Available: " + 
                    AvailableItems.Where(x => x.Type == APWorldData.StaticItems.SwapRandom).Count().ToString();
                lblAvailableSSP.Text = "Available: " + 
                    AvailableItems.Where(x => x.Type == APWorldData.StaticItems.SwapPick).Count().ToString();
                lblAvailableLD.Text = "Available: " + 
                    AvailableItems.Where(x => x.Type == APWorldData.StaticItems.LowerDifficulty).Count().ToString();
            }
        }


        private void btnPurchaseSwapRand_Click(object sender, EventArgs e) =>
            Purchase(APWorldData.StaticItems.SwapRandom, ExtraAPFunctionalityHelper.SwapSongRandomPrice);

        private void btnPurchaseSwapPick_Click(object sender, EventArgs e) =>
            Purchase(APWorldData.StaticItems.SwapPick, ExtraAPFunctionalityHelper.SwapSongPickPrice);

        private void btnPurchaseLowerDif_Click(object sender, EventArgs e) => 
            Purchase(APWorldData.StaticItems.LowerDifficulty, ExtraAPFunctionalityHelper.LowerDifficultyPrice);

        private void Purchase(APWorldData.StaticItems item, long Price)
        {
            if (!ExtraAPFunctionalityHelper.TryPurchaseItem(connection, config, item, Price))
                MessageBox.Show("Not enough energy!");
            ClientSyncHelper_ConstantCallback();
        }

    }
}
