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
                Console.WriteLine("S: Print Available Songs\nF: Use Filler Item\nEnd: Exit Program");
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.End:
                        ConsoleExiting = true;
                        break;
                    case ConsoleKey.S:
                        PrintSongs();
                        break;
                    case ConsoleKey.F:
                        UseFillerItem();
                        break;
                }
            }
        }

        private static void UseFillerItem()
        {
            Console.Clear();
            int RandomSwapsTotal = connection.ReceivedStaticItems.TryGetValue(APWorldData.StaticItems.SwapRandom, out int rst) ? rst : 0;
            int RandomSwapsUsed = config!.UsedFiller.TryGetValue(APWorldData.StaticItems.SwapRandom, out int rsu) ? rsu : 0;
            int RandomSwapsAvailable = RandomSwapsTotal - RandomSwapsUsed;

            int SwapsTotal = connection.ReceivedStaticItems.TryGetValue(APWorldData.StaticItems.SwapPick, out int st) ? st : 0;
            int SwapsUsed = config!.UsedFiller.TryGetValue(APWorldData.StaticItems.SwapPick, out int su) ? su : 0;
            int SwapsAvailable = SwapsTotal - SwapsUsed;

            int LowerDiffTotal = connection.ReceivedStaticItems.TryGetValue(APWorldData.StaticItems.LowerDifficulty, out int lt) ? lt : 0;
            int LowerDiffUsed = config!.UsedFiller.TryGetValue(APWorldData.StaticItems.LowerDifficulty, out int lu) ? lu : 0;
            int LowerDiffAvailable = LowerDiffTotal - LowerDiffUsed;

            Console.WriteLine($"P: Use Swap Song (Pick) {SwapsAvailable}");
            Console.WriteLine($"R: Use Swap Song (Random) {RandomSwapsAvailable}");
            Console.WriteLine($"D: Lower Minimum Difficulty {LowerDiffAvailable}");
            Console.WriteLine($"S: Lower Score Requirement {LowerDiffAvailable}");
            Console.WriteLine("BackSpace: Go Back");

            var Key = Console.ReadKey();
            switch (Key.Key)
            {
                case ConsoleKey.Backspace:
                    return;
                case ConsoleKey.P:
                    //UsePick();
                    break;
                case ConsoleKey.R:
                    //UsePick();
                    break;
                case ConsoleKey.D:
                    //UsePick();
                    break;
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

        static void ApplyUIListeners(ConnectionData connectionData) { }
    }
}