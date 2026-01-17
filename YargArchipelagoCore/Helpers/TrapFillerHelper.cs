using System.Reflection;
using TDMUtils;
using YargArchipelagoCore.Data;
using YargArchipelagoCommon;
using static YargArchipelagoCore.Data.APWorldData;
using System.Diagnostics;

namespace YargArchipelagoCore.Helpers
{
    public static class TrapFillerHelper
    {
        public static bool SendPendingTrapOrFiller(ConnectionData Connection, ConfigData Config)
        {
            bool SongLoadBuffer = (DateTime.Now - Connection.LastSongStarted).TotalSeconds >= 10;
            if (!Connection.GetPacketServer().IsConnected || !Connection.IsCurrentlyPlayingSong(out _) || !SongLoadBuffer) return false;
            foreach (var Item in Connection.ApItemsRecieved)
            {
                if (!Item.Type.IsTrapOrFiller() || Config.ApItemsUsed.Contains(Item)) continue;
                Debug.WriteLine($"Sending Filler {Item.Type}");
                SendOneTrapFiller(Connection, Config, Item);
            }
            return true;
        }

        public static void SendOneTrapFiller(ConnectionData Connection, ConfigData Config, StaticYargAPItem item)
        {
            var Type = GetFillerTrapType(item.Type);
            var Sender = Connection.GetSession().Players.GetPlayerName(item.SendingPlayerSlot)??"Unknown";
            _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket { ActionItem = new CommonData.ActionItemData(Type, Sender) });
            Config.ApItemsUsed.Add(item);
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
