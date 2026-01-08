using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net;
using YargArchipelagoCore.Data;
using YargArchipelagoCore.Helpers;
using Newtonsoft.Json;
using YargArchipelagoCommon;

namespace YargArchipelagoCLI
{
    internal class NewConnectionHelper
    {
        public static ConnectionData? CreateNewConnection()
        {
            string? CachedAddress = null;
            string? CachedName = null;
            string? CachedPassword = null;
            if (File.Exists(CommonData.ConnectionCachePath))
            {
                try
                {
                    var TempConnection = JsonConvert.DeserializeObject<ConnectionData>(File.ReadAllText(CommonData.ConnectionCachePath));
                    CachedAddress = TempConnection!.Address;
                    CachedName = TempConnection!.SlotName;
                    CachedPassword = TempConnection!.Password;
                }
                catch { }
            }
        Start:
            Console.Clear();
            var RawIP = ConsoleHelper.ReadLineWithDefault("Server IP/port", CachedAddress);
            var (Ip, Port) = NetworkHelpers.ParseIpAddress(RawIP);
            if (Ip is null)
            {
                Console.WriteLine("Invalid IP");
                goto Start;
            }
            var SlotName = ConsoleHelper.ReadLineWithDefault("AP slot name", CachedName);
            var Password = ConsoleHelper.ReadLineWithDefault("AP password", CachedPassword, true);

            Console.Clear();
            ArchipelagoSession session = ArchipelagoSessionFactory.CreateSession(Ip, Port);
            Console.WriteLine($"Connecting to {SlotName}@{session.Socket.Uri}");
            ConnectionData data = new(RawIP, SlotName, Password, session);

            var result = session.TryConnectAndLogin("YAYARG", data.SlotName, ItemsHandlingFlags.AllItems, APWorldData.APVersion, ["AP"], null, data.Password);

            Console.Clear();
            if (result is LoginFailure failure)
            {
                Console.WriteLine($"Failed to connect to server:\n{string.Join("\n", failure.Errors)}\nPress any key to try again");
                Console.ReadKey();
                goto Start;
            }
            else if (result is not LoginSuccessful)
            {
                Console.WriteLine($"Failed to connect to server: {result.GetType()}\nPress any key to try again");
                Console.ReadKey();
                goto Start;
            }
            Console.WriteLine($"Connected to {session.Socket.Uri}");

            return data;
        }
    }


}
