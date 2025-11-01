using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Helpers;
using YargArchipelagoCommon;
using YargArchipelagoCore.Data;
using static YargArchipelagoCore.Helpers.MultiplatformHelpers;

namespace YargArchipelagoCLI
{
    public class ConfigCreator(ConnectionData Connection)
    {
        private ConfigData data = new();
        private readonly List<SongPool> Pools = [];
        private Dictionary<int, PlandoData> PlandoSongData;
        private SongPoolManager SongPoolManager;

        public ConfigData? CreateConfig()
        {
            data.ParseAPLocations(Connection.GetSession());
            if (!SongImporter.TryReadSongs(out var SongData)) { return null; }
            data.SongData = SongData;
            PlandoSongData = data.GetSongIndexes().ToDictionary(x => x, x => new PlandoData { SongNum = x });
            SongPoolManager = new(Pools, PlandoSongData, data);

            CreateAndEditPool();

            PlandoManager plandoManager = new(PlandoSongData, SongPoolManager, data, Connection, Pools);

            ConsoleSelect<Action> PoolSelector;
            int CurrentSelection = 0, CurrentPage = 0;
            while (true)
            {
                PoolSelector = new();
                PoolSelector.AddText(SectionPlacement.Pre, $"Active Song Pools: {Pools.Count}").StartIndex(CurrentSelection).StartPage(CurrentPage)
                    .AddText(SectionPlacement.Pre, $"{SongPoolManager.GetTotalPotentialSongAmount()}/{data.TotalAPSongLocations} Song slots filled")
                    .AddSeparator(SectionPlacement.Pre);
                foreach (var i in Pools)
                    PoolSelector.Add(i.Name, () => { EditPool(i); });
                PoolSelector.AddStatic(new string('=', Console.WindowWidth), ReturnFlag.Unselectable);
                PoolSelector.AddStatic("Edit Plando", plandoManager.EditPladoDate);
                PoolSelector.AddStatic("New Pool", CreateAndEditPool);
                PoolSelector.AddStatic("Delete Pool", SelectAndDeletePool);
                PoolSelector.AddStatic("Confirm", ReturnFlag.Confirm, () => SongPoolManager.GetTotalPotentialSongAmount() == data.TotalAPSongLocations);
                PoolSelector.AddStatic("Exit Program", ReturnFlag.Cancel);

                var Selection = PoolSelector.GetSelection();
                if (Selection.WasCancelation())
                    if (MessageBox.Show("Are you sure you want to exit?", buttons: MessageBoxButtons.YesNo) == DialogResult.Yes)
                        return null;
                if (Selection.WasConfirmation() && confirmValidConfig())
                    return data;
                Selection.Tag?.Invoke();
                CurrentSelection = PoolSelector.CurrentSelection;
                CurrentPage = PoolSelector.CurrentPage;
            }

            void CreateAndEditPool()
            {
                var NewP = CreateNewPool();
                if (NewP is not null)
                {
                    EditPool(NewP);
                    Pools.Add(NewP);
                }
            }

        }

        public void SelectAndDeletePool()
        {
            Console.WriteLine("NYI");
            Console.ReadLine();
        }

        public bool confirmValidConfig()
        {
            int AddedSongs = SongPoolManager.GetTotalPotentialSongAmount();
            int SongsNeeded = data.TotalAPSongLocations;
            if (SongPoolManager.GetTotalPotentialSongAmount() != data.TotalAPSongLocations)
            {
                MessageBox.Show($"You must add a total of at least {SongsNeeded} songs across all song pools.\n" +
                    $"You have added {AddedSongs} songs across {Pools.Count} song pools.",
                    "Invalid song Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            var Success = ClientInitializationHelper.AssignSongs(data, Connection, Pools, PlandoSongData, SongPoolManager);
            return Success;
        }

        public SongPool? CreateNewPool()
        {
            Console.WriteLine("Enter Pool Name:");
            string PoolName = Console.ReadLine()!;

            ConsoleSelect<CommonData.SupportedInstrument> InstrumentConsoleSelect = new();
            InstrumentConsoleSelect.AddText(SectionPlacement.Pre, "select an Instrument:").AddSeparator(SectionPlacement.Pre);
            foreach (CommonData.SupportedInstrument i in Enum.GetValues(typeof(CommonData.SupportedInstrument)))
                InstrumentConsoleSelect.Add(i.ToString(), i);
            var Selected = InstrumentConsoleSelect.GetSelection();
            if (Selected.WasCancelation())
                return null;
            var SelectedInstrument = Selected.Tag;

            SongPool newPool = new(PoolName, SelectedInstrument);
            return newPool;
        }

        public SongPool? EditPool(SongPool Pool)
        {
            ConsoleSelect<Action> PoolConfigConsoleSelect;
            while (true)
            {
                PoolConfigConsoleSelect = new();
                PoolConfigConsoleSelect.SetCancelKey(ConsoleKey.None)
                    .AddText(SectionPlacement.Pre, $"Pool: {Pool.Name}")
                    .AddText(SectionPlacement.Pre, $"Instrument: {Pool.Instrument}")
                    .AddText(SectionPlacement.Pre, $"Available Songs: {Pool.GetAvailableSongs(data.SongData).Count}")
                    .AddSeparator(SectionPlacement.Pre);
                PoolConfigConsoleSelect.Add($"Random Amount: {Pool.RandomAmount}", () => Pool.RandomAmount = !Pool.RandomAmount);
                string AmountDisplay = Pool.RandomAmount ? "Pool Weight" : "Amount in pool";
                PoolConfigConsoleSelect.Add($"{AmountDisplay}: {Pool.AmountInPool}", () => Pool.AmountInPool = GetNumber(SongPoolManager.GetTotalAmountAssignableToThisPoolViaConfig(Pool)));
                PoolConfigConsoleSelect.Add($"Min Difficulty: {Pool.MinDifficulty}", () => Pool.MinDifficulty = GetNumber(100));
                PoolConfigConsoleSelect.Add($"Max Difficulty: {Pool.MaxDifficulty}", () => Pool.MaxDifficulty = GetNumber(100));
                PoolConfigConsoleSelect.Add($"Reward 1 Score Requirement: {Pool.CompletionRequirement.Reward1Req}", 
                    () => Pool.CompletionRequirement.Reward1Req = CLIHelpers.NextEnumValue(Pool.CompletionRequirement.Reward1Req));
                PoolConfigConsoleSelect.Add($"Reward 1 Minimum Difficulty: {Pool.CompletionRequirement.Reward1Diff}",
                    () => Pool.CompletionRequirement.Reward1Diff = CLIHelpers.NextEnumValue(Pool.CompletionRequirement.Reward1Diff));
                PoolConfigConsoleSelect.Add($"Reward 2 Score Requirement: {Pool.CompletionRequirement.Reward1Req}",
                    () => Pool.CompletionRequirement.Reward1Req = CLIHelpers.NextEnumValue(Pool.CompletionRequirement.Reward1Req));
                PoolConfigConsoleSelect.Add($"Reward 2 Minimum Difficulty: {Pool.CompletionRequirement.Reward1Diff}",
                    () => Pool.CompletionRequirement.Reward1Diff = CLIHelpers.NextEnumValue(Pool.CompletionRequirement.Reward1Diff));
                PoolConfigConsoleSelect.AddStatic("Confirm", ReturnFlag.Confirm);

                var Selection = PoolConfigConsoleSelect.GetSelection();
                if (Selection.WasConfirmation())
                    return Pool;
                Selection.Tag?.Invoke();
            }

        }

        public static int GetNumber(int Max)
        {
            Console.Clear();
            Console.WriteLine("Enter Value:");
            Enter:
            var Value = Console.ReadLine();

            if (!int.TryParse(Value, out var Number) || Number < 0 || Number > Max)
            {
                Console.WriteLine("Invalid Number!");
                goto Enter;
            }
            return Number;
        }
    }

    public class PlandoManager(Dictionary<int, PlandoData> PlandoSongData, SongPoolManager SongPoolManager, ConfigData data, ConnectionData Connection, List<SongPool> Pools)
    {
        private PlandoData? GetPlandoDataForSong(int Song)
        {
            if (!PlandoSongData.TryGetValue(Song, out var data)) return null;
            return data;
        }

        public void EditPladoDate()
        {
            while (true)
            {
                var SelectedSongLocation = PickSongLocation();
                if (SelectedSongLocation == null) return;

                PlandoSongData.SetIfEmpty(SelectedSongLocation.SongNumber, new PlandoData());
                var SelectedPlandoData = PlandoSongData[SelectedSongLocation.SongNumber];

                ConsoleSelect<Action> EditPladoSelector;

                while (true)
                {
                    EditPladoSelector = new();
                    EditPladoSelector.AddText(SectionPlacement.Pre, $"Song: {SelectedSongLocation.SongNumber}").AddSeparator(SectionPlacement.Pre).AddCancelOption("Go Back")
                        .Add($"Pool Plando Enabled: {SelectedPlandoData.PoolPlandoEnabled}", 
                        () => SelectedPlandoData.PoolPlandoEnabled = !SelectedPlandoData.PoolPlandoEnabled, 
                        () => Pools.Count > 0 && SongPoolManager.CanPlandoAnyPoolToThisLocation(SelectedPlandoData))

                        .Add($"Assign Plando Pool: {SelectedPlandoData.SongPool}", 
                        () => SelectedPlandoData.SongPool = PickNewPool(SelectedPlandoData), 
                        () => SelectedPlandoData.PoolPlandoEnabled)

                        .Add($"Song Plando Enabled: {SelectedPlandoData.SongPlandoEnabled}", 
                        () => SelectedPlandoData.SongPlandoEnabled = !SelectedPlandoData.SongPlandoEnabled,
                        () => SongPoolManager.GetValidSongsForAllPools().Count > 0 && SongPoolManager.CanPlandoSongToThisLocation(SelectedPlandoData))

                        .Add($"Assign Plando Song: {DisplayPlandoSong(data.SongData, SelectedPlandoData)}", 
                        () => SelectedPlandoData.SongHash = PickNewSong(SelectedPlandoData), 
                        () => SelectedPlandoData.SongPlandoEnabled);

                    if (!EditPladoSelector.HasValidOptions)
                    {
                        Console.WriteLine($"Cannot Enable plando for this location");
                        Console.ReadKey();
                        break;
                    }

                    var Selected = EditPladoSelector.GetSelection();
                    if (Selected.WasCancelation()) break;
                    Selected.Tag?.Invoke();
                }
            }

        }

        private string DisplayPlandoSong(Dictionary<string, CommonData.SongData> SongData, PlandoData Selected)
        {
            if (Selected.SongHash is null) return "None";
            if (!SongData.TryGetValue(Selected.SongHash, out var song)) return "None";
            return song.GetSongDisplayName();
        }

        private string? PickNewSong(PlandoData selectedPlandoData)
        {
            HashSet<CommonData.SongData> ValidSongs = selectedPlandoData.ValidSongsForThisPlando(Pools, data);
            ConsoleSelect<CommonData.SongData> EditPladoSelector = new();
            foreach(var i in ValidSongs.OrderBy(x => x.GetSongDisplayName()))
                EditPladoSelector.Add(i.GetSongDisplayName(), i);
            var Selected = EditPladoSelector.GetSelection();
            if (Selected.WasCancelation()) return null;
            return Selected.Tag?.SongChecksum;
        }

        private string? PickNewPool(PlandoData selectedPlandoData)
        {
            HashSet<SongPool> validPools = selectedPlandoData.ValidPoolsForThisPlando(Pools, data);
            ConsoleSelect<SongPool> EditPladoSelector = new();
            foreach (var i in validPools)
                EditPladoSelector.Add(i.Name, i);
            var Selected = EditPladoSelector.GetSelection();
            if (Selected.WasCancelation()) return null;
            return Selected.Tag?.Name;
        }

        private SongLocation? PickSongLocation()
        {
            ConsoleSelect<SongLocation> Location = new();
            Location.AddText(SectionPlacement.Pre, "Pick A location to Plando").AddSeparator(SectionPlacement.Pre).AddCancelOption("Go Back");
            Location.Add($"{SongPoolManager.SongDisplay(data.GoalSong.SongNumber)}{GetPlandoStateString(PlandoSongData, data.GoalSong)}", data.GoalSong);
            foreach (var i in data!.ApLocationData.OrderBy(x => x.Key))
                Location.Add($"{SongPoolManager.SongDisplay(i.Value.SongNumber)}{GetPlandoStateString(PlandoSongData, i.Value)}", i.Value);
            var Selected = Location.GetSelection();
            if (Selected.WasCancelation()) return null;
            return Selected.Tag;
        }

        private string GetPlandoStateString(Dictionary<int, PlandoData> PlandoSongData, SongLocation i)
        {
            List<string> Tags = [];
            var Data = GetPlandoDataForSong(i.SongNumber);
            if (Data is null) return "";
            if (Data.HasValidPoolPlando) Tags.Add("P");
            if (Data.HasValidSongPlando) Tags.Add("S");
            if (Tags.Count > 0) return $" [{string.Join(", ", Tags)}]";
            return "";
        }
    }
}
