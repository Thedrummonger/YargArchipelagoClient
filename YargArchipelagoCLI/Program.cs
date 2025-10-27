using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Helpers;
using YargArchipelagoCore.Helpers;
using static YargArchipelagoCore.Helpers.MultiplatformHelpers;
using TDMUtils;
using YargArchipelagoCommon;

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
                        var ShowDetails = PrintSongs(true);
                        if (ShowDetails is null) continue;
                        Console.Clear();
                        Console.WriteLine($"Song: {ShowDetails.GetSongDisplayName(config)}\nProfile: {ShowDetails.Requirements.Name}\nInstrument: {ShowDetails.Requirements.Instrument}");
                        Console.WriteLine(ShowDetails.Requirements.CompletionRequirement.ToFormattedJson());
                        Console.ReadKey();
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
            Console.WriteLine("Press escape to cancel");

        Select:
            var Key = Console.ReadKey();
            switch (Key.Key)
            {
                case ConsoleKey.Escape:
                    return;
                case ConsoleKey.P when SwapsAvailable > 1 || config.CheatMode:
                    SwapSong(true);
                    break;
                case ConsoleKey.R when RandomSwapsAvailable > 1 || config.CheatMode:
                    SwapSong(false);
                    break;
                case ConsoleKey.D when LowerDiffAvailable > 1 || config.CheatMode:
                    LowerScore(true);
                    break;
                case ConsoleKey.S when LowerDiffAvailable > 1 || config.CheatMode:
                    LowerScore(false);
                    break;
                default:
                    Console.WriteLine("Invalid Selection");
                    goto Select;
            }
        }

        private static void LowerScore(bool Difficulty)
        {
            Console.Clear();
            SongLocation? song = PrintSongs(true);
            if (song is null) return;
            FillerActivationHelper fillerActivationHelper = new(connection, config, song);

            Dictionary<ConsoleKey, (Action, string)> ValidActions = [];

            if (song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward1Diff > CommonData.SupportedDifficulty.Easy)
                ValidActions[ConsoleKey.D1] = (fillerActivationHelper.LowerReward1Diff, "Lower Reward 1 min Difficulty");
            if (song.HasExtraCheck(out _) && song.Requirements!.CompletionRequirement.Reward2Diff > CommonData.SupportedDifficulty.Easy)
                ValidActions[ConsoleKey.D2] = (fillerActivationHelper.LowerReward1Diff, "Lower Reward 2 min Difficulty");

            if (song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward1Req > APWorldData.CompletionReq.Clear)
                ValidActions[ConsoleKey.D3] = (fillerActivationHelper.LowerReward1Req, "Lower Reward 1 min Score");
            if (song.HasExtraCheck(out _) && song.Requirements!.CompletionRequirement.Reward2Req > APWorldData.CompletionReq.Clear)
                ValidActions[ConsoleKey.D4] = (fillerActivationHelper.LowerReward2Req, "Lower Reward 2 min Score");

            if (ValidActions.Count < 1)
            {
                Console.WriteLine("Location difficulty can not be lowered any further!");
                Console.ReadLine();
                return;
            }

            Console.WriteLine(song.GetSongDisplayName(config));
            foreach (var i in ValidActions)
                Console.WriteLine($"{i.Key}. {i.Value.Item2}");

            Console.WriteLine("Select a requirement to change. Press escape to cancel");
            while (true)
            {
                var selection = Console.ReadKey();
                if (selection.Key == ConsoleKey.Escape)
                    return;
                if (ValidActions.TryGetValue(selection.Key, out var SelectedSong))
                {
                    SelectedSong.Item1();
                    Console.WriteLine($"Song {song.SongNumber} Difficulty Updated\n{song.Requirements!.CompletionRequirement.ToFormattedJson()}");
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine("Invalid Selection");
            }
        }

        private static void SwapSong(bool AllowPick)
        {
            SongLocation? song = PrintSongs(true);
            if (song is null) return;
            FillerActivationHelper fillerActivationHelper = new(connection, config, song);
            var AvailableSongs = fillerActivationHelper.GetValidSongReplacements();
            if (AvailableSongs.Length < 1)
            {
                Console.WriteLine("No valid Replacements for song");
                return;
            }

            CommonData.SongData? SelectedSong = null;
            if (AllowPick)
                SelectedSong = GetReplacementSong(AvailableSongs);
            else
                SelectedSong = AvailableSongs[connection.GetRNG().Next(AvailableSongs.Length)];

            if (SelectedSong is null)
                return;

            var OldSong = song.GetSongDisplayName(config);
            song.SongHash = SelectedSong.SongChecksum;
            Console.WriteLine($"[{OldSong}] swapped to [{song.GetSongDisplayName(config)}]");

            var ItemUsed = AllowPick ? APWorldData.StaticItems.SwapPick : APWorldData.StaticItems.SwapRandom;
            if (!config.CheatMode)
            {
                config!.UsedFiller.SetIfEmpty(ItemUsed, 0);
                config!.UsedFiller[ItemUsed]++;
            }

            config.SaveConfigFile(connection);
            connection.GetPacketServer().SendClientStatusPacket();
            Console.ReadKey();
        }

        public static CommonData.SongData? GetReplacementSong(CommonData.SongData[] validReplacements)
        {
            Dictionary<int, CommonData.SongData> LocationDict = [];
            Console.Clear();
            Console.WriteLine("Available replacements");

            int Index = 0;
            foreach (var i in validReplacements.OrderBy(x => x.GetSongDisplayName()))
            {
                Console.WriteLine($"{Index}. {i.GetSongDisplayName()}");
                LocationDict[Index] = i;
                Index++;
            }
            Console.WriteLine("Type a song number to select..\nType exit to cancel");
            while (true)
            {
                var Id = Console.ReadLine();
                if (Id.ToLower() == "exit")
                    return null;
                if (int.TryParse(Id, out int IntID) && LocationDict.TryGetValue(IntID, out var selected))
                    return selected;
                Console.WriteLine("Invalid Selection");
            }
        }

        public static SongLocation? PrintSongs(bool GetSelection = false)
        {
            Dictionary<int, SongLocation> LocationDict = [];
            Console.Clear();
            Console.WriteLine("Available Songs");

            var GoalSongAvailable = config.GoalSong.SongAvailableToPlay(connection, config);
            if (GoalSongAvailable || config.DebugPrintAllSongs)
            {
                int Num = config.GoalSong.SongNumber;
                string Debug = config.DebugPrintAllSongs ? !config.GoalSong.HasUncheckedLocations(connection) ? "@ " : (GoalSongAvailable ? "O " : "X ") : "";
                Console.WriteLine($"{Debug}{Num}. Goal Song: {config.GoalSong.GetSongDisplayName(config!)} [{config.GoalSong.Requirements!.Name}]");
                LocationDict[Num] = config.GoalSong;
            }
            foreach (var i in config!.ApLocationData.OrderBy(x => x.Key))
            {
                int Num = i.Value.SongNumber;
                var SongAvailable = i.Value.SongAvailableToPlay(connection, config);
                if (!SongAvailable && !config.DebugPrintAllSongs)
                    continue;

                string Debug = config.DebugPrintAllSongs ? !i.Value.HasUncheckedLocations(connection) ? "@ " : SongAvailable ? "O " : "X " : "";
                Console.WriteLine($"{Debug}{Num}. {i.Value.GetSongDisplayName(config!)} [{i.Value.Requirements!.Name}]");
                LocationDict[Num] = i.Value;
            }
            if (!GetSelection)
            {
                Console.WriteLine("Press enter to go back..");
                Console.ReadLine();
                return null;
            }
            Console.WriteLine("Type a song number to select..\nleave blank and press enter to cancel..");
            while (true)
            {
                var Id = Console.ReadLine();
                if (Id.ToLower() == string.Empty)
                    return null;
                if (int.TryParse(Id, out int IntID) && LocationDict.TryGetValue(IntID, out var selected))
                    return selected;
                Console.WriteLine("Invalid Selection");
            }
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