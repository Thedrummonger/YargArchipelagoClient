using System.Reflection;
using TDMUtils;
using YargArchipelagoCore.Data;
using YargArchipelagoCommon;
using static YargArchipelagoCore.Data.APWorldData;

namespace YargArchipelagoCore.Helpers
{
    public static class TrapFillerHelper
    {
        public static void SendPendingTrapOrFiller(ConnectionData Connection, ConfigData Config)
        {
            if (Connection.IsCurrentlyPlayingSong(out _)) return;
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
            _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket { ActionItem = new CommonData.ActionItemData(Type) });
            Config.ProcessedTrapsFiller.SetIfEmpty(item, 0);
            Config.ProcessedTrapsFiller[item]++;
            Config.SaveConfigFile(Connection);
        }

        public static CommonData.APActionItem GetFillerTrapType(this StaticItems item)
            => typeof(StaticItems)
            .GetMember(item.ToString())
            .FirstOrDefault()?
            .GetCustomAttribute<FillerTrapTypeAttribute>(false)?.Type ?? 
            CommonData.APActionItem.NonFiller;

        public static bool IsTrapOrFiller(this StaticItems item) => item.GetFillerTrapType() != CommonData.APActionItem.NonFiller;
    }


}
