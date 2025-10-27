using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Helpers;
using YargArchipelagoCore.Helpers;
using static YargArchipelagoCore.Helpers.MultiplatformHelpers;

namespace YargArchipelagoCLI
{
    internal static class Program
    {
        private static ConnectionData connection;
        private static ConfigData config;
        static void Main(string[] args)
        {
            CliMessageBox.ApplyMessageBoxAction();
            if (!ClientInitializationHelper.ConnectSession(NewConnectionHelper.CreateNewConnection, CreateNewConfig, ApplyUIListeners, out connection, out config))
            {
                var key = Console.ReadKey();
                return;
            }
            bool ConsoleExiting = false;
            while (!ConsoleExiting)
            {
                Console.Clear();
                Console.WriteLine("S: Print Available Songs\nEnd: Exit Program");
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.End:
                        ConsoleExiting = true;
                        break;
                    case ConsoleKey.S:
                        PrintSongs();
                        break;
                }
            }
        }

        public static void PrintSongs()
        {
            Console.Clear();
            Console.WriteLine("Available Songs");

            var GoalSongAvailable = config.GoalSong.SongAvailableToPlay(connection, config);
            if (GoalSongAvailable || config.DebugPrintAllSongs)
            {
                string Num = config.GoalSong.SongNumber.ToString();
                string Debug = config.DebugPrintAllSongs ? !config.GoalSong.HasUncheckedLocations(connection) ? "@ " : (GoalSongAvailable ? "O " : "X ") : "";
                Console.WriteLine($"{Debug}({Num})Goal Song: {config.GoalSong.GetSongDisplayName(config!)} [{config.GoalSong.Requirements!.Name}]");
            }
            foreach (var i in config!.ApLocationData.OrderBy(x => x.Key))
            {
                string Num = i.Value.SongNumber.ToString();
                var SongAvailable = i.Value.SongAvailableToPlay(connection, config);
                if (!SongAvailable && !config.DebugPrintAllSongs)
                    continue;

                string Debug = config.DebugPrintAllSongs ? !i.Value.HasUncheckedLocations(connection) ? "@ " : SongAvailable ? "O " : "X " : "";
                Console.WriteLine($"{Debug}({Num}){i.Value.GetSongDisplayName(config!)} [{i.Value.Requirements!.Name}]");
            }
            Console.ReadLine();
        }

        public static ConfigData? CreateNewConfig()
        {
            ConfigCreator configCreator = new ConfigCreator(connection);
            if (configCreator.HasErrored)
                return null;
            return null;
        }

        static void ApplyUIListeners(ConnectionData connectionData)
        {

        }
    }
}