using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMUtils;
using TDMUtils.CLITools;
using YargArchipelagoCore.Data;
using YargArchipelagoCore.Helpers;

namespace YargArchipelagoCLI
{
    public class EnergyLinkShop(ConnectionData connection, ConfigData config)
    {
        public void ShowEnergyLinkShop()
        {
            int CurrentSelection = 0;
            ConsoleSelect<Action> EnergyShop;

            connection.clientSyncHelper.ConstantCallback += PrintCurrentEnergy;
            while (true)
            {
                HashSet<APWorldData.StaticYargAPItem> AvailableItems = [.. connection!.ApItemsRecieved, .. config!.ApItemsPurchased];
                AvailableItems = [.. AvailableItems.Where(x => !config.ApItemsUsed.Contains(x))];
                EnergyShop = new();
                EnergyShop.AddCancelOption("Go Back").AddText(SectionPlacement.Pre, "Current Energy: ").AddSeparator(SectionPlacement.Pre).StartIndex(CurrentSelection);
                AddPurchaseOption(EnergyShop, APWorldData.StaticItems.SwapRandom, ExtraAPFunctionalityHelper.SwapSongRandomPrice, AvailableItems);
                AddPurchaseOption(EnergyShop, APWorldData.StaticItems.SwapPick, ExtraAPFunctionalityHelper.SwapSongPickPrice, AvailableItems);
                AddPurchaseOption(EnergyShop, APWorldData.StaticItems.LowerDifficulty, ExtraAPFunctionalityHelper.LowerDifficultyPrice, AvailableItems);

                Console.Clear();
                var Selection = EnergyShop.GetSelection();
                CurrentSelection = EnergyShop.CurrentSelection;
                if (Selection.WasCancelation())
                    break;
                Selection.Tag!();
            }
            connection.clientSyncHelper.ConstantCallback -= PrintCurrentEnergy;
        }

        private void AddPurchaseOption(ConsoleSelect<Action> menu, APWorldData.StaticItems item, long Price, HashSet<APWorldData.StaticYargAPItem> AvailableItems)
        {
            string ItemName = APWorldData.StaticItems.SwapRandom.GetDescription();
            string ItemPrice = ExtraAPFunctionalityHelper.FormatLargeNumber(Price);
            string CurrentItems = AvailableItems.Where(x => x.Type == item).Count().ToString();
            string MenuText = $"Purchase {ItemName} [Current {CurrentItems}]: {ItemPrice}";
            menu.Add(MenuText, () => Purchase(item, Price));
        }

        private void Purchase(APWorldData.StaticItems item, long Price)
        {
            long Energy = ExtraAPFunctionalityHelper.GetEnergy(connection, config);
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"Current Energy: {Energy}".PadRight(Console.WindowWidth));
            if (!ExtraAPFunctionalityHelper.TryPurchaseItem(connection, config, item, Price))
            {
                Console.SetCursorPosition(0, 1);
                Console.WriteLine($"Not Enough energy to purchase {item.GetDescription()} at Price {ExtraAPFunctionalityHelper.FormatLargeNumber(Price)}");
                Console.ReadKey(true);
            }
        }

        private void PrintCurrentEnergy()
        {
            long Energy = ExtraAPFunctionalityHelper.GetEnergy(connection, config);
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"Current Energy: {Energy:N0}");
        }
    }
}
