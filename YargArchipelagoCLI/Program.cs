using YargArchipelagoCore.Data;
using YargArchipelagoCore.Helpers;
using TDMUtils;
using YargArchipelagoCommon;
using System.Diagnostics;
using static YargArchipelagoCore.Helpers.MultiplatformHelpers;
using TDMUtils.CLITools;
using Archipelago.MultiClient.Net.MessageLog.Messages;

namespace YargArchipelagoCLI
{
    internal static class Program
    {
        static bool ConsoleExiting = false;
        private static ConnectionData connection;
        private static ConfigData config;

        private static Action<LogMessage>? LogAPChat = null;

        static AppletScreen? liveMonitor = null;
        static void Main(string[] args)
        {
            MultiplatformHelpers.MessageBox.ApplyConsoleTemplate();
            if (!ClientInitializationHelper.ConnectSession(NewConnectionHelper.CreateNewConnection, () => ConfigCreator.CreateConfig(connection), ApplyUIListeners, out connection, out config))
                return;
            liveMonitor = new(CreateApplets());
            Console.Clear();
            ConsoleSelect<Action> consoleSelect = new();
            consoleSelect.AddCancelOption("Exit Program").AddText(SectionPlacement.Pre, "Yarg AP Client").AddSeparator(SectionPlacement.Pre)
                .Add("Open Live Monitor", liveMonitor.Show)
                .Add("Show Available Songs", PrintCurrentSongList)
                .Add("Use Filler Item", UseFillerItem)
                .Add("Toggle Config", ToggleConfig)
                .Add("Rescan Songs", () => SongImporter.RescanSongs(config!, connection!))
                .Add("Sync With YARG", connection!.GetPacketServer().SendClientStatusPacket);

            liveMonitor.Show();
            while (!ConsoleExiting)
            {
                Console.Clear();
                var Selection = consoleSelect.GetSelection();
                if (ConsoleExiting)
                    break;
                if (Selection.WasCancelation())
                    if (MessageBox.Show("Are you sure you want to exit?", buttons: MessageBoxButtons.YesNo) == DialogResult.Yes)
                        break;
                Selection.Tag?.Invoke();
            }
        }

        private static Applet[] CreateApplets()
        {
            List<Applet> applets = [
                new StatusApplet(connection, config),
                new SongApplet(connection, config),
            ];
            var ChatApplet = new ChatApplet();
            LogAPChat = (L) => 
            {
                ColoredString coloredString = new();
                foreach (var i in L.Parts)
                    coloredString.AddText(i.Text, i.Color.ConvertToSystemColor(), false);
                ChatApplet.LogChat(coloredString);
            };
            applets.Add(ChatApplet);
            return [.. applets];
        }

        static void ApplyUIListeners(ConnectionData connectionData)
        {
            connection.GetSession().Items.ItemReceived += (_) => liveMonitor?.FlagForUpdate<SongApplet>();
            connection.GetSession().MessageLog.OnMessageReceived += (M) => { LogAPChat?.Invoke(M); liveMonitor?.FlagForUpdate<ChatApplet>(); };
            connection.GetPacketServer().ConnectionChanged += () => liveMonitor?.FlagForUpdate<StatusApplet>();
            connection.GetPacketServer().CurrentSongUpdated += () => liveMonitor?.FlagForUpdate<StatusApplet>();
        }

        private static void ToggleConfig()
        {
            ConsoleSelect<Action> consoleSelect;

            int CurrentSelection = 0;
            while (true)
            {
                consoleSelect = new();
                consoleSelect.AddCancelOption("Go Back").AddText(SectionPlacement.Pre, "Toggle Config Options..").AddSeparator(SectionPlacement.Pre).StartIndex(CurrentSelection)
                    //.Add($"BroadCast Song Names: {config.BroadcastSongName}", () => config.BroadcastSongName = !config.BroadcastSongName)
                    .Add($"DeathLink [YAML: {config.YamlDeathLink.GetDescription()}]: {config.DeathLinkMode.GetDescription()}", () => { 
                        config.DeathLinkMode = EnumerableUtilities.NextValue(config.DeathLinkMode); 
                        connection.UpdateDeathLinkTags(config);
                        })
                    .Add($"Item Notifications: {config.InGameItemLog.GetDescription()}", () => config.InGameItemLog = EnumerableUtilities.NextValue(config.InGameItemLog))
                    .Add($"Chat Notifications: {config.InGameAPChat}", () => config.InGameAPChat = !config.InGameAPChat)
                    .Add($"Cheat Mode: {config.CheatMode}", () => config.CheatMode = !config.CheatMode, () => Debugger.IsAttached);
                Console.Clear();
                var Selection = consoleSelect.GetSelection();
                CurrentSelection = consoleSelect.CurrentSelection;
                if (Selection.WasCancelation())
                    break;
                Selection.Tag!();
            }
        }

        private static void PrintCurrentSongList()
        {
            var ShowDetails = PrintAvailableSongs(["Select A song to see info..", "This list does not auto update as new songs are found, use the Live Monitor!\""], [], 
                new FlaggedOption<SongLocation>("Cancel", ReturnFlag.Cancel));
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
            if (Selection.WasCancelation())
                return;
            Selection.Tag!();
        }

        private static void LowerScore()
        {
            Console.Clear();
            SongLocation? song = PrintAvailableSongs([$"Select song to lower score.."], [], new FlaggedOption<SongLocation>("Cancel", ReturnFlag.Cancel));
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
            if (Selection.WasCancelation())
                return;
            Selection.Tag!();

            Console.WriteLine($"Song {song.SongNumber} Difficulty Updated\n{song.Requirements!.CompletionRequirement.ToFormattedJson()}");
            Console.ReadLine();
        }

        private static void SwapSong(bool AllowPick)
        {
            SongLocation? song = PrintAvailableSongs([$"Select song to swap.."], [], new FlaggedOption<SongLocation>("Cancel", ReturnFlag.Cancel));
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
            consoleSelect.AddText(SectionPlacement.Pre, $"Select A song to replace {song.GetSongDisplayName(config)}").AddSeparator(SectionPlacement.Pre).AddCancelOption("Cancel");

            foreach (var i in validReplacements.OrderBy(x => x.GetSongDisplayName()))
                consoleSelect.Add(i.GetSongDisplayName(), i);
            var Selection = consoleSelect.GetSelection();
            if (Selection.WasCancelation())
                return null;
            return Selection.Tag;
        }

        public static SongLocation? PrintAvailableSongs(string[] preText, string[] postText, params Option<SongLocation>[] StaticOptions)
        {
            Dictionary<int, SongLocation> LocationDict = [];
            Console.Clear();

            ConsoleSelect<SongLocation> consoleSelect = new();

            foreach (var i in preText) consoleSelect.AddText(SectionPlacement.Pre, i);
            if (preText.Any()) consoleSelect.AddSeparator(SectionPlacement.Pre);
            if (postText.Any()) consoleSelect.AddSeparator(SectionPlacement.Post);
            foreach (var i in postText) consoleSelect.AddText(SectionPlacement.Post, i);
            foreach (var i in StaticOptions) consoleSelect.AddStatic(i);

            var GoalSongAvailable = config.GoalSong.SongAvailableToPlay(connection, config);
            string GoalSongDebugHeader = config.DebugPrintAllSongs ? !config.GoalSong.HasUncheckedLocations(connection) ? "@ " : (GoalSongAvailable ? "O " : "X ") : "";
            consoleSelect.Add($"{config.GoalSong.SongNumber}. " + GoalSongDebugHeader + config.GoalSong.GetSongDisplayName(config), config.GoalSong, () => GoalSongAvailable || config.DebugPrintAllSongs);

            foreach (var i in config!.ApLocationData.OrderBy(x => x.Key))
            {
                int Num = i.Value.SongNumber;
                var SongAvailable = i.Value.SongAvailableToPlay(connection, config);
                string SongDebugHeader = config.DebugPrintAllSongs ? !i.Value.HasUncheckedLocations(connection) ? "@ " : SongAvailable ? "O " : "X " : "";
                consoleSelect.Add($"{i.Value.SongNumber}. {SongDebugHeader}{i.Value.GetSongDisplayName(config)} [{i.Value.Requirements?.Name}]", i.Value, () => SongAvailable || config.DebugPrintAllSongs);
            }
            var Selection = consoleSelect.GetSelection();
            if (Selection.WasCancelation())
                return null;
            return Selection.Tag;
        }
    }
}