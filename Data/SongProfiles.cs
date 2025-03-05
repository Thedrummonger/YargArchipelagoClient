namespace YargArchipelagoClient.Data
{
    public class SongProfile(string name, Constants.Instrument instrument)
    {
        public string Name = name;
        public Constants.Instrument instrument = instrument;
        public int AmountInPool = 0;
        public int MinDifficulty = 3;
        public int MaxDifficulty = 6;
        public CompletionRequirement CompletionRequirement = new();
        public override string ToString()
        {
            return Name;
        }

        public Dictionary<string, SongData> GetAvailableSongs(Dictionary<string, SongData> SongData)
        {
            var Data = SongData.Where(x =>
                x.Value.TryGetDifficulty(instrument, out var difficulty) &&
                difficulty >= MinDifficulty &&
                difficulty <= MaxDifficulty);
            return Data.ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public class CompletionRequirement()
    {
        public Constants.CompletionReq Reward1Req = Constants.CompletionReq.Clear;
        public Constants.CompletionReq Reward2Req = Constants.CompletionReq.ThreeStar;
        public Constants.Difficulty Reward1Diff = Constants.Difficulty.Expert;
        public Constants.Difficulty Reward2Diff = Constants.Difficulty.Expert;
    }
}
