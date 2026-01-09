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
        private const long SwapSongRandomPrice = 17_000_000_000;
        private const long SwapSongPickPrice = 20_000_000_000;
        private const long LowerDifficultyPrice = 15_000_000_000;
        private MainForm ParentForm;
        private ConnectionData connection;
        private ConfigData config;
        public EnergyShop(MainForm Parent)
        {
            InitializeComponent();
            ParentForm = Parent;
            lblLowerDiffPrice.Text = $"Price: {FormatLargeNumber(LowerDifficultyPrice)}";
            lblSwapSongPickPrice.Text = $"Price: {FormatLargeNumber(SwapSongPickPrice)}";
            lblSwapSongRandPrice.Text = $"Price: {FormatLargeNumber(SwapSongRandomPrice)}";
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
                long Energy = CheckLocationHelpers.GetEnergy(connection, config);
                HashSet<APWorldData.StaticYargAPItem> AvailableItems = [.. connection!.ApItemsRecieved, .. config!.ApItemsPurchased];
                lblCurEnergyVal.Text = Energy.ToString("N0");

                btnPurchaseLowerDif.Enabled = Energy >= LowerDifficultyPrice;
                btnPurchaseSwapPick.Enabled = Energy >= SwapSongPickPrice;
                btnPurchaseSwapRand.Enabled = Energy >= SwapSongRandomPrice;

                AvailableItems = [.. AvailableItems.Where(x => !config.ApItemsUsed.Contains(x))];
                lblAvailableSSR.Text = "Available: " + 
                    AvailableItems.Where(x => x.Type == APWorldData.StaticItems.SwapRandom).Count().ToString();
                lblAvailableSSP.Text = "Available: " + 
                    AvailableItems.Where(x => x.Type == APWorldData.StaticItems.SwapPick).Count().ToString();
                lblAvailableLD.Text = "Available: " + 
                    AvailableItems.Where(x => x.Type == APWorldData.StaticItems.LowerDifficulty).Count().ToString();
            }
        }

        public static string FormatLargeNumber(long number)
        {
            if (number >= 1_000_000_000_000)
                return (number / 1_000_000_000_000.0).ToString("0.##") + " Trillion";
            if (number >= 1_000_000_000)
                return (number / 1_000_000_000.0).ToString("0.##") + " Billion";
            if (number >= 1_000_000)
                return (number / 1_000_000.0).ToString("0.##") + " Million";
            if (number >= 1_000)
                return (number / 1_000.0).ToString("0.##") + " Thousand";

            return number.ToString("N0");
        }

        private void tryPurchaseItem(APWorldData.StaticItems Type, long Price)
        {
            var CurrentEnergy = CheckLocationHelpers.GetEnergy(connection, config);
            if (!CheckLocationHelpers.SpendEnergy(connection, config, Price))
            {
                MessageBox.Show($"Not Enough Energy!");
                return;
            }
            var CurCount = config.ApItemsPurchased.Where(x => x.Type == Type).Count();
            config.ApItemsPurchased.Add(new(Type, APWorldData.APIDs.IDFromStaticItem[Type], -99, CurCount, "YARGAPSHOP"));
        }

        private void btnPurchaseSwapRand_Click(object sender, EventArgs e) =>
            tryPurchaseItem(APWorldData.StaticItems.SwapRandom, SwapSongRandomPrice);

        private void btnPurchaseSwapPick_Click(object sender, EventArgs e) =>
            tryPurchaseItem(APWorldData.StaticItems.SwapPick, SwapSongPickPrice);

        private void btnPurchaseLowerDif_Click(object sender, EventArgs e) =>
            tryPurchaseItem(APWorldData.StaticItems.LowerDifficulty, LowerDifficultyPrice);
    }
}
