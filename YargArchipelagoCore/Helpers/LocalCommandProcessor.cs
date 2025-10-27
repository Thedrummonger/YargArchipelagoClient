using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YargArchipelagoClient.Data;
using YargArchipelagoCommon;

namespace YargArchipelagoClient.Helpers
{
    public class LocalCommandProcessor
    {
        public static void ProcessCommand(ConfigData Config, ConnectionData Connection, Action<string> Log, Action RefreshSongList, string v)
        {
            switch (v.ToLower())
            {
                case "star":
                    Log($"Simulating start power item");
                    _ = Connection.GetPacketServer()?.SendPacketAsync(new CommonData.Networking.YargAPPacket { ActionItem = new(CommonData.APActionItem.StarPower) });
                    break;
                case "restart":
                    Log($"Simulating restart trap");
                    _ = Connection.GetPacketServer()?.SendPacketAsync(new CommonData.Networking.YargAPPacket { ActionItem = new(CommonData.APActionItem.Restart) });
                    break;
                case "dl":
                    Log($"Simulating Death Link");
                    _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket
                    {
                        deathLinkData = new CommonData.DeathLinkData { Source = "Self", Cause = "Command" }
                    });
                    break;
                case "songs":
                    Config.DebugPrintAllSongs = !Config.DebugPrintAllSongs;
                    Log($"Showing all songs {Config.DebugPrintAllSongs}");
                    RefreshSongList();
                    break;
                case "cheat":
                    Config.CheatMode = !Config.CheatMode;
                    Log($"Cheat Mode Active {Config.CheatMode}");
                    RefreshSongList();
                    break;
                default:
                    Log($"{v} is not a valid command");
                    break;
            }
        }
    }
}
