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
            consoleSelect.AddCancelOption("Exit Program").AddText(SectionPlacement.Pre, "Yarg AP Client").AddSeparator(SectionPlacement.Pre)
                .Add("Show Available Songs", PrintCurrentSongList)
                .Add("Use Filler Item", UseFillerItem)
                .Add("Toggle Config", ToggleConfig)
                .Add("Rescan Songs", () => SongImporter.RescanSongs(config!, connection!))
                .Add("Sync With YARG", connection!.GetPacketServer().SendClientStatusPacket);

            while (!ConsoleExiting)
            {
                Console.Clear();
                var Selection = consoleSelect.GetSelection();
                if (Selection is FlaggedOption<Action> flag && flag.Flag is ReturnFlag.Cancel)
                    break;
                Selection.Tag!();
            }
        }

        private static void ToggleConfig()
        {
            ConsoleSelect<Action> consoleSelect;

            int CurrentSelection = 0;
            while (true)
            {
                consoleSelect = new();
                consoleSelect.AddCancelOption("Go Back").AddText(SectionPlacement.Pre, "Toggle Config Options..").AddSeparator(SectionPlacement.Pre).StartIndex(CurrentSelection)
                    .Add($"BroadCast Song Names: {config.BroadcastSongName}", () => config.BroadcastSongName = !config.BroadcastSongName)
                    .Add($"DeathLink: {config.deathLinkEnabled}", () => config.deathLinkEnabled = !config.deathLinkEnabled, () => config.ServerDeathLink)
                    .Add($"Item Notifications: {config.InGameItemLog}", () => config.InGameItemLog = CycleLog(config.InGameItemLog))
                    .Add($"Chat Notifications: {config.InGameAPChat}", () => config.InGameAPChat = !config.InGameAPChat)
                    .Add($"Cheat Mode: {config.CheatMode}", () => config.CheatMode = !config.CheatMode, () => Debugger.IsAttached);
                Console.Clear();
                var Selection = consoleSelect.GetSelection();
                CurrentSelection = consoleSelect.CurrentSelection;
                if (Selection is FlaggedOption<Action> flag && flag.Flag is ReturnFlag.Cancel)
                    break;
                Selection.Tag!();
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

            ConsoleSelect<Action> consoleSelect = new();
            consoleSelect.AddCancelOption("Cancel").AddText(SectionPlacement.Pre, "Select an item to use..").AddSeparator(SectionPlacement.Pre)
            .Add($"Use Swap Song (Pick) {SwapsAvailable}", () => SwapSong(true), () => SwapsAvailable > 1 || config.CheatMode)
            .Add($"Use Swap Song (Random) {RandomSwapsAvailable}", () => SwapSong(false), () => RandomSwapsAvailable > 1 || config.CheatMode)
            .Add($"Lower Song requirements {LowerDiffAvailable}", () => LowerScore(), () => LowerDiffAvailable > 1 || config.CheatMode);

            var Selection = consoleSelect.GetSelection();
            if (Selection is FlaggedOption<Action> flag && flag.Flag is ReturnFlag.Cancel)
                return;
            Selection.Tag!();
        }

        private static void LowerScore()
        {
            Console.Clear();
            SongLocation? song = PrintAvailableSongs($"Select song to lower score..");
            Console.Clear();
            if (song is null) return;
            FillerActivationHelper fillerActivationHelper = new(connection, config, song);

            ConsoleSelect<Action> consoleSelect = new();
            consoleSelect.AddText(SectionPlacement.Pre, $"{song.GetSongDisplayName(config)}\nCurrent Requirements:\n{song.Requirements!.CompletionRequirement.ToFormattedJson()}")
            .AddSeparator(SectionPlacement.Pre).AddText(SectionPlacement.Pre, "Select a requirement to change:").AddSeparator(SectionPlacement.Pre)
            .Add("Lower Reward 1 min Difficulty", fillerActivationHelper.LowerReward1Diff,
                () => { return song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward1Diff > CommonData.SupportedDifficulty.Easy; })
            .Add("Lower Reward 2 min Difficulty", fillerActivationHelper.LowerReward2Diff,
                () => { return song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward2Diff > CommonData.SupportedDifficulty.Easy; })
            .Add("Lower Reward 1 min Score", fillerActivationHelper.LowerReward1Req,
                () => { return song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward1Req > APWorldData.CompletionReq.Clear; })
            .Add("Lower Reward 2 min Score", fillerActivationHelper.LowerReward2Req,
                () => { return song.HasStandardCheck(out _) && song.Requirements!.CompletionRequirement.Reward2Req > APWorldData.CompletionReq.Clear; });

            if (!consoleSelect.HasValidOptions)
            {
                Console.WriteLine("Location difficulty can not be lowered any further!");
                Console.ReadLine();
                return;
            }

            var Selection = consoleSelect.GetSelection();
            if (Selection is FlaggedOption<Action> flag && flag.Flag is ReturnFlag.Cancel)
                return;
            Selection.Tag!();

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
            if (Selection is FlaggedOption<CommonData.SongData> flag && flag.Flag is ReturnFlag.Cancel)
                return null;
            return Selection.Tag;
        }

        public static SongLocation? PrintAvailableSongs(string SelectionText)
        {
            Dictionary<int, SongLocation> LocationDict = [];
            Console.Clear();

            ConsoleSelect<SongLocation> consoleSelect = new();

            consoleSelect.AddText(SectionPlacement.Pre, "Available songs").AddText(SectionPlacement.Pre, SelectionText).AddSeparator(SectionPlacement.Pre);

            var GoalSongAvailable = config.GoalSong.SongAvailableToPlay(connection, config);
            string GoalSongDebugHeader = config.DebugPrintAllSongs ? !config.GoalSong.HasUncheckedLocations(connection) ? "@ " : (GoalSongAvailable ? "O " : "X ") : "";
            consoleSelect.Add(GoalSongDebugHeader + config.GoalSong.GetSongDisplayName(config), config.GoalSong, () => GoalSongAvailable || config.DebugPrintAllSongs);

            foreach (var i in config!.ApLocationData.OrderBy(x => x.Key))
            {
                int Num = i.Value.SongNumber;
                var SongAvailable = i.Value.SongAvailableToPlay(connection, config);
                string SongDebugHeader = config.DebugPrintAllSongs ? !i.Value.HasUncheckedLocations(connection) ? "@ " : SongAvailable ? "O " : "X " : "";
                consoleSelect.Add(SongDebugHeader + i.Value.GetSongDisplayName(config), i.Value, () => SongAvailable || config.DebugPrintAllSongs);
            }
            var Selection = consoleSelect.GetSelection();
            if (Selection is FlaggedOption<SongLocation> flag && flag.Flag is ReturnFlag.Cancel)
                return null;
            return Selection.Tag;
        }

        public static ConfigData? CreateNewConfig()
        {
            ConfigCreator configCreator = new ConfigCreator(connection);
            return null;
        }

        static void ApplyUIListeners(ConnectionData connectionData) { }
    }
}