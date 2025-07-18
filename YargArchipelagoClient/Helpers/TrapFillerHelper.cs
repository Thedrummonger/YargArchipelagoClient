using System.Reflection;
using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoCommon;
using static YargArchipelagoClient.Data.APWorldData;

namespace YargArchipelagoClient.Helpers
{
    public static class TrapFillerHelper
    {
        public static void SendPendingTrapOrFiller(ConnectionData Connection, ConfigData Config)
        {
            if (Connection.CurrentlyPlaying is null) return;
            foreach(var Item in Connection.ReceivedStaticItems)
            {
                if (!Item.Key.IsTrapOrFiller() || Item.Value < 1) continue;
                var AmountAlreadySent = Config.ProcessedTrapsFiller.TryGetValue(Item.Key, out var R) ? R : 0;
                if (Item.Value > AmountAlreadySent)
                    SendOneTrapFiller(Connection, Config, Item.Key);
            }
        }

        public static void SendOneTrapFiller(ConnectionData Connection, ConfigData Config, APWorldData.StaticItems item)
        {
            var Type = GetFillerTrapType(item);
            _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket { ActionItem = new CommonData.TrapData(Type) });
            Config.ProcessedTrapsFiller.SetIfEmpty(item, 0);
            Config.ProcessedTrapsFiller[item]++;
            Config.SaveConfigFile(Connection);
        }

        public static CommonData.FillerTrapType GetFillerTrapType(this StaticItems item)
            => typeof(StaticItems)
            .GetMember(item.ToString())
            .FirstOrDefault()?
            .GetCustomAttribute<FillerTrapTypeAttribute>(false)?.Type ?? 
            CommonData.FillerTrapType.NonFiller;

        public static bool IsTrapOrFiller(this StaticItems item) => item.GetFillerTrapType() != CommonData.FillerTrapType.NonFiller;
    }


}
