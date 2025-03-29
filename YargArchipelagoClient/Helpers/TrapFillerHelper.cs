using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YargArchipelagoClient.Data;
using YargArchipelagoCommon;

namespace YargArchipelagoClient.Helpers
{
    class TrapFillerHelper()
    {
        public static void SendPendingTrapOrFiller(ConnectionData Connection, ConfigData Config)
        {
            foreach(var Item in Connection.ReceivedFiller)
            {
                CommonData.trapType PacketType;
                switch (Item.Key)
                {
                    case APWorldData.StaticItems.StarPower:
                        PacketType = CommonData.trapType.StarPower;
                        break;
                    case APWorldData.StaticItems.TrapRestart:
                        PacketType = CommonData.trapType.Restart;
                        break;
                    default:
                        return;
                }
                if (Item.Value < 1) continue;
                var AmountAlreadySent = Config.TrapsRegistered.TryGetValue(Item.Key, out var R) ? R : 0;
                if (Item.Value > AmountAlreadySent)
                    _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket { trapData = new CommonData.TrapData(PacketType) });

                if (AmountAlreadySent != Item.Value)
                {
                    Config.TrapsRegistered[Item.Key] = Item.Value;
                    Config.SaveConfigFile(Connection);
                }
            }
        }
    }
}
