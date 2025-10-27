using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YargArchipelagoClient.Data;
using YargArchipelagoCommon;
using static YargArchipelagoCore.Helpers.MultiplatformHelpers;

namespace YargArchipelagoCore.Helpers
{
    internal class DeathLinkHelper
    {
        public static Action<DeathLink> ManualDeathlinkHandler = (deathLink) => { Debug.WriteLine($"Deathlink {deathLink.Source}"); };
        public static void HandleDeathlinkRecieved(DeathLink deathLink, ConnectionData Connection, ConfigData Config)
        {
            if (!Config!.deathLinkEnabled) return;
            if (Config.ManualMode)
                ManualDeathlinkHandler(deathLink);
            else
            {
                _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket
                {
                    deathLinkData = new CommonData.DeathLinkData { Source = deathLink.Source, Cause = deathLink.Cause }
                });
            }
        }
    }
}
