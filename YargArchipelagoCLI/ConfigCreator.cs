using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            SongPoolManager = new(Pools, PlandoSongData, data.TotalAPSongLocations, data.SongData);

            CreateAndEditPool();

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
                PoolSelector.AddStatic("New Pool", CreateAndEditPool);
                PoolSelector.AddStatic("Delete Pool", SelectAndDeletePool);
                PoolSelector.AddStatic("Confirm", ReturnFlag.Confirm, () => SongPoolManager.GetTotalPotentialSongAmount() == data.TotalAPSongLocations);
                PoolSelector.AddStatic("ExitProgram", ReturnFlag.Cancel);

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
}
