using YargArchipelagoCore.Data;
using YargArchipelagoCommon;

namespace YargArchipelagoCore.Helpers
{
    public class LocalCommandProcessor
    {
        public static void ProcessCommand(ConfigData Config, ConnectionData Connection, Action<string> Log, Action RefreshSongList, string v)
        {
            switch (v.ToLower())
            {
                case "star":
                    Log($"Simulating start power item");
                    _ = Connection.GetPacketServer()?.SendPacketAsync(new CommonData.Networking.YargAPPacket { ActionItem = new(CommonData.APActionItem.StarPower, "Debug") });
                    break;
                case "rock":
                    Log($"Simulating rock meter trap");
                    _ = Connection.GetPacketServer()?.SendPacketAsync(new CommonData.Networking.YargAPPacket { ActionItem = new(CommonData.APActionItem.RockMeterTrap, "Debug") });
                    break;
                case "restart":
                    Log($"Simulating restart trap");
                    _ = Connection.GetPacketServer()?.SendPacketAsync(new CommonData.Networking.YargAPPacket { ActionItem = new(CommonData.APActionItem.Restart, "Debug") });
                    break;
                case "dl":
                    Log($"Simulating Death Link");
                    _ = Connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket
                    {
                        deathLinkData = new CommonData.DeathLinkData("Server", "Command")
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
                case "energy":
                    var energyToAdd = ExtraAPFunctionalityHelper.ScaleEnergyValue(Connection, Config, 200_000);
                    var ELKey = ExtraAPFunctionalityHelper.EnergyLinkKey(Connection.GetSession());
                    Log($"Adding {energyToAdd} energy to key {ELKey}");
                    var Session = Connection.GetSession();
                    Session.DataStorage[ELKey].Initialize(0);
                    Session.DataStorage[ELKey] += energyToAdd;
                    Log(Session.DataStorage[ELKey]);
                    break;
                default:
                    Log($"{v} is not a valid command");
                    break;
            }
        }
    }
}
