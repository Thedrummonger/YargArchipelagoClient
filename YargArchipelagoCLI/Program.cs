using YargArchipelagoClient.Data;
using YargArchipelagoClient.Helpers;
using YargArchipelagoCore.Helpers;
using TDMUtils;
using YargArchipelagoCommon;
using System.Diagnostics;

namespace YargArchipelagoCLI
{
    internal static class Program
    {
        private static ConnectionData connection;
        private static ConfigData config;
        static void Main(string[] args)
        {
            MultiplatformHelpers.MessageBox.ApplyConsoleTemplate();
            if (!ClientInitializationHelper.ConnectSession(NewConnectionHelper.CreateNewConnection, CreateNewConfig, ApplyUIListeners, out connection, out config))
            {
                var key = Console.ReadKey();
                return;
            }
            Console.Clear();
            bool ConsoleExiting = false;
            ConsoleSelect<Action> consoleSelect = new();
            consoleSelect.IncludeCancel("Exit Program").AddText(SectionPlacement.Pre, "Yarg AP Client").AddSeparator(SectionPlacement.Pre)
                .Add("Show Available Songs", PrintCurrentSongList)
                .Add("Use Filler Item", UseFillerItem)
                .Add("Toggle Config", ToggleConfig)
                .Add("Rescan Songs", () => SongImporter.RescanSongs(config!, connection!))
                .Add("Sync With YARG", connection!.GetPacketServer().SendClientStatusPacket);

            while (!ConsoleExiting)
            {
                Console.Clear();
                var Selection = consoleSelect.GetSelection();
                if (Selection is null)
                    break;
                Selection.Tag();
            }
        }

        private static void ToggleConfig()
        {
            CLIKeyChoiceContainer choiceContainer;
            while (true)
            {
                Console.Clear();
                choiceContainer = new("Config", ConsoleKey.Escape);
                choiceContainer.AddOption(ConsoleKey.B, $"BroadCast Song Names: {config.BroadcastSongName}", () => config.BroadcastSongName = !config.BroadcastSongName);
                choiceContainer.AddOption(ConsoleKey.D, $"DeathLink: {config.deathLinkEnabled}", () => config.deathLinkEnabled = !config.deathLinkEnabled);
                choiceContainer.AddOption(ConsoleKey.I, $"Item Notifications: {config.InGameItemLog}", () => config.InGameItemLog = CycleLog(config.InGameItemLog));
                choiceContainer.AddOption(ConsoleKey.C, $"Chat Notifications: {config.InGameAPChat}", () => config.InGameAPChat = !config.InGameAPChat);
                choiceContainer.AddOption(ConsoleKey.M, $"Cheat Mode: {config.CheatMode}", () => config.CheatMode = !config.CheatMode, () => Debugger.IsAttached);

                var Choice = choiceContainer.GetChoice();
                if (Choice is null) return;
                Choice.OnSelect?.Invoke();
                config.SaveConfigFile(connection);
            }

            CommonData.ItemLog CycleLog(CommonData.ItemLog Cur) => Cur switch
            {
                CommonData.ItemLog.None => CommonData.ItemLog.ToMe,
                CommonData.ItemLog.ToMe => CommonData.ItemLog.All,
                CommonData.ItemLog.All => CommonData.ItemLog.None,
                _ => CommonData.ItemLog.None,
            };
        }

        private static void PrintCurrentSongList()
        {
            var ShowDetails = PrintAvailableSongs($"Select A song to see info..");
            if (ShowDetails is null) return;
            Console.Clear();
            Console.WriteLine($"Song: {ShowDetails.GetSongDisplayName(config)}\nProfile: {ShowDetails.Requirements?.Name}\nInstrument: {ShowDetails.Requirements?.Instrument}");
            Console.WriteLine(ShowDetails.Requirements?.CompletionRequirement?.ToFormattedJson());
            Console.ReadKey();
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

            CLIKeyChoiceContainer choiceContainer = new("Select a filler item:");
            choiceContainer.AddOption(ConsoleKey.P, $"Use Swap Song (Pick) {SwapsAvailable}", () => SwapSong(true), () => SwapsAvailable > 1 || config.CheatMode);
            choiceContainer.AddOption(ConsoleKey.R, $"Use Swap Song (Random) {RandomSwapsAvailable}", () => SwapSong(false), () => RandomSwapsAvailable > 1 || config.CheatMode);
            choiceContainer.AddOption(ConsoleKey.L, $"Lower Song requirements {LowerDiffAvailable}", () => LowerScore(), () => LowerDiffAvailable > 1 || config.CheatMode);

            choiceContainer.GetChoice()?.OnSelect?.Invoke();
        }

        private static void LowerScore()
        {
            Console.Clear();
            SongLocation? song = PrintAvailableSongs($"Select song to lower score..");
            Console.Clear();
            if (song is null) return;
            FillerActivationHelper fillerActivationHelper = new(connection, config, song);

            CLIKeyChoiceContainer choiceContainer = new($"{song.GetSongDisplayName(config)}\nCurrent Requirements:\n{song.Requirements!.CompletionRequirement.ToFormattedJson()}");
            choiceContainer.SetSelectText("Select a requirement to change:");

            choiceContainer.AddOption(ConsoleKey.D1, "Lower Reward 1 min Difficulty", fillerActivationHelper.LowerReward1Diff, 
                () => { return song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward1Diff > CommonData.SupportedDifficulty.Easy; });
            choiceContainer.AddOption(ConsoleKey.D2, "Lower Reward 2 min Difficulty", fillerActivationHelper.LowerReward2Diff,
                () => { return song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward2Diff > CommonData.SupportedDifficulty.Easy; });
            choiceContainer.AddOption(ConsoleKey.D3, "Lower Reward 1 min Score", fillerActivationHelper.LowerReward1Req,
                () => { return song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward1Req > APWorldData.CompletionReq.Clear; });
            choiceContainer.AddOption(ConsoleKey.D4, "Lower Reward 2 min Score", fillerActivationHelper.LowerReward2Req,
                () => { return song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward2Req > APWorldData.CompletionReq.Clear; });

            if (!choiceContainer.AreAnyValid())
            {
                Console.WriteLine("Location difficulty can not be lowered any further!");
                Console.ReadLine();
                return;
            }

            var Choice = choiceContainer.GetChoice();
            if (Choice is null)
                return;

            Choice?.OnSelect?.Invoke();
            Console.WriteLine($"Song {song.SongNumber} Difficulty Updated\n{song.Requirements!.CompletionRequirement.ToFormattedJson()}");
            Console.ReadLine();
        }

        private static void SwapSong(bool AllowPick)
        {
            SongLocation? song = PrintAvailableSongs($"Select A song to swap");
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
                SelectedSong = GetReplacementSong(song, AvailableSongs);
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

        public static CommonData.SongData? GetReplacementSong(SongLocation song, CommonData.SongData[] validReplacements)
        {
            Dictionary<int, CommonData.SongData> LocationDict = [];
            Console.Clear();

            ConsoleSelect<CommonData.SongData> consoleSelect = new();
            consoleSelect.AddText(SectionPlacement.Pre, $"Select A song to replace {song.GetSongDisplayName(config)}").AddSeparator(SectionPlacement.Pre);

            int Index = 0;
            foreach (var i in validReplacements.OrderBy(x => x.GetSongDisplayName()))
                consoleSelect.Add(i.GetSongDisplayName(), i);
            var Selection = consoleSelect.GetSelection();
            if (Selection is null) return null;
            return Selection.Tag;
        }

        public static SongLocation? PrintAvailableSongs(string SelectionText)
        {
            Dictionary<int, SongLocation> LocationDict = [];
            Console.Clear();

            CLITextChoiceContainer choiceContainer = new("Available songs", string.Empty);
            choiceContainer.SetSelectText(SelectionText);

            var GoalSongAvailable = config.GoalSong.SongAvailableToPlay(connection, config);
            string GoalSongDebugHeader = config.DebugPrintAllSongs ? !config.GoalSong.HasUncheckedLocations(connection) ? "@ " : (GoalSongAvailable ? "O " : "X ") : "";
            choiceContainer.AddOption(config.GoalSong.SongNumber.ToString(), GoalSongDebugHeader + config.GoalSong.GetSongDisplayName(config), null, 
                () => GoalSongAvailable || config.DebugPrintAllSongs, config.GoalSong);

            foreach (var i in config!.ApLocationData.OrderBy(x => x.Key))
            {
                int Num = i.Value.SongNumber;
                var SongAvailable = i.Value.SongAvailableToPlay(connection, config);
                string SongDebugHeader = config.DebugPrintAllSongs ? !i.Value.HasUncheckedLocations(connection) ? "@ " : SongAvailable ? "O " : "X " : "";
                choiceContainer.AddOption(i.Value.SongNumber.ToString(), SongDebugHeader + i.Value.GetSongDisplayName(config), null,
                    () => SongAvailable || config.DebugPrintAllSongs, i.Value);
            }
            var Choice = choiceContainer.GetChoice()?.tag;
            if (Choice is SongLocation SongChoice)
                return SongChoice;
            return null;
        }

        public static ConfigData? CreateNewConfig()
        {
            ConfigCreator configCreator = new ConfigCreator(connection);
            return null;
        }

        static void ApplyUIListeners(ConnectionData connectionData) { }
    }
}