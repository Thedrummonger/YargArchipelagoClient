namespace YargArchipelagoClient.Data
{
    public class SongProfile(string name, Constants.Instrument instrument)
    {
        public string Name = name;
        public Constants.Instrument instrument = instrument;
        public int AmountInPool = 0;
        public int MinDifficulty = 3;
        public int MaxDifficulty = 6;
        public Constants.CompletionReq CompletionRequirement = Constants.CompletionReq.Clear;
        public Constants.CompletionReq ExtraRequirement = Constants.CompletionReq.FiveStar;
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
}
