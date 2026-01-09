using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YargArchipelagoCommon;
using YargArchipelagoCore.Data;
using static YargArchipelagoCore.Helpers.MultiplatformHelpers;

namespace YargArchipelagoCore.Helpers
{
    public static class ExtraAPFunctionalityHelper
    {
        public const long minEnergyLinkScale = 20000;
        public const long maxEnergyLinkScale = 1000000;

        public const long SwapSongRandomPrice = 17_000_000_000;
        public const long SwapSongPickPrice = 20_000_000_000;
        public const long LowerDifficultyPrice = 15_000_000_000;
        public static bool TryPurchaseItem(ConnectionData connection, ConfigData config, APWorldData.StaticItems Type, long Price)
        {
            var CurrentEnergy = GetEnergy(connection, config);
            if (!TryUseEnergy(connection, config, Price))
                return false;
            var CurCount = config.ApItemsPurchased.Where(x => x.Type == Type).Count();
            config.ApItemsPurchased.Add(new(Type, APWorldData.APIDs.IDFromStaticItem[Type], -99, CurCount, "YARGAPSHOP"));
            return true;
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
        public static void SendEnergy(ConnectionData connection, ConfigData config, long amount, bool WasLocationChecked)
        {
            if (config.EnergyLinkMode <= CommonData.EnergyLinkType.None) return;
            if (config.EnergyLinkMode == CommonData.EnergyLinkType.CheckSong && !WasLocationChecked) return;
            if (config.EnergyLinkMode == CommonData.EnergyLinkType.OtherSong && WasLocationChecked) return;

            var Session = connection.GetSession();
            string EnergyLinkKey = $"EnergyLink{Session.Players.ActivePlayer.Team}";
            Session.DataStorage[EnergyLinkKey].Initialize(0);
            Session.DataStorage[EnergyLinkKey] += ScaleEnergyValue(connection, config, amount);
        }

        public static long ScaleEnergyValue(ConnectionData connection, ConfigData config, long baseAmount)
        {
            int AmountOfLocationsTotal = connection.GetSession().Locations.AllLocations.Count;
            int AmountOfLocationsChecked = connection.GetSession().Locations.AllLocationsChecked.Count;
            double completionPercentage = AmountOfLocationsChecked / AmountOfLocationsTotal;
            double scale = minEnergyLinkScale + (completionPercentage * (maxEnergyLinkScale - minEnergyLinkScale));
            long Energy = (long)(baseAmount * scale);
            return Energy;
        }

        public static long GetEnergy(ConnectionData connection, ConfigData config)
        {
            if (config.EnergyLinkMode <= CommonData.EnergyLinkType.None) return 0;
            var Session = connection.GetSession();
            string EnergyLinkKey = $"EnergyLink{Session.Players.ActivePlayer.Team}";
            Session.DataStorage[EnergyLinkKey].Initialize(0);
            return Session.DataStorage[EnergyLinkKey];
        }

        public static bool TryUseEnergy(ConnectionData connection, ConfigData config, long Amount)
        {
            if (config.EnergyLinkMode <= CommonData.EnergyLinkType.None) return false;
            var Session = connection.GetSession();
            string EnergyLinkKey = $"EnergyLink{Session.Players.ActivePlayer.Team}";
            Session.DataStorage[EnergyLinkKey].Initialize(0);
            if (Session.DataStorage[EnergyLinkKey] >= Amount)
            {
                Session.DataStorage[EnergyLinkKey] -= Amount;
                return true;
            }
            return false;

        }
    }
}
