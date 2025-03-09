using Archipelago.MultiClient.Net;

namespace YargArchipelagoClient.Data
{
    public class ConfigData()
    {
        /// <summary>
        /// A list of songs and their data pulled from the given song folder
        /// </summary>
        public Dictionary<string, CommonData.SongData> SongData { get; set; } = [];
        /// <summary>
        /// A mapping of the song "number" to it's AP data and mapped song
        /// </summary>
        public Dictionary<int, SongLocation> ApLocationData { get; } = [];
        /// <summary>
        /// The goal song info
        /// </summary>
        public SongLocation GoalSong = new(0);

        public Dictionary<string, int> UsedFiller = [];

        public bool BroadcastSongName = false;

        public bool ManualMode = false;

        public int TotalSongsInPool => ApLocationData.Count + 1; // +1 For Goal Song

        public void ParseAPLocations(ArchipelagoSession archipelagoSession)
        {
            foreach (var i in archipelagoSession.Locations.AllLocations)
            {
                var Name = archipelagoSession.Locations.GetLocationNameFromId(i);
                if (Name == "Goal Song")
                {
                    GoalSong.APStandardCheckLocation = i;
                    continue;
                }
                var data = Name.Split(" ");
                if (data.Length != 4 || !int.TryParse(data[1], out var songNum)) throw new Exception($"Malformed Song Name! {Name}");
                if (!ApLocationData.ContainsKey(songNum)) ApLocationData[songNum] = new(songNum);
                switch (data[3].Trim())
                {
                    case "1":
                        ApLocationData[songNum].APStandardCheckLocation = i;
                        break;
                    case "2":
                        ApLocationData[songNum].APExtraCheckLocation = i;
                        break;
                    case "Point":
                        ApLocationData[songNum].APFameCheckLocation = i;
                        break;
                    default: throw new Exception($"Malformed Song Name! {Name}");
                }
            }
        }
    }

    public class SongLocation(int num)
    {
        public int SongNumber = num;
        public string? SongHash = null;
        public long? APStandardCheckLocation = null;
        public long? APExtraCheckLocation = null;
        public long? APFameCheckLocation = null;
        public SongPool? Requirements = null;
        public bool HasStandardCheck(out long ID)
        {
            ID = APStandardCheckLocation ?? -1;
            return APStandardCheckLocation is not null;
        }
        public bool HasExtraCheck(out long ID)
        {
            ID = APExtraCheckLocation ?? -1;
            return APExtraCheckLocation is not null;
        }
        public bool HasFameCheck(out long ID)
        {
            ID = APFameCheckLocation ?? -1;
            return APFameCheckLocation is not null;
        }
        public string GetSongDisplayName(ConfigData config, bool WithArtist = true, bool WithAlbum = false, bool WithSongNum = false)
        {
            var Data = GetSongData(config);
            if (Data is not CommonData.SongData SongData) return SongNumber.ToString();
            string Display = SongData.Name;
            if (WithArtist)
                Display += $" by {SongData.Artist}";
            if (WithAlbum)
                Display += $" from {SongData.Album}";
            if (WithSongNum)
                Display = $"[Song {SongNumber}] {Display}";
            return Display;
        }
        public CommonData.SongData? GetSongData(ConfigData config)
        {
            if (config is null) return null;
            if (SongHash is null) return null;
            if (!config.SongData.TryGetValue(SongHash, out var SongData)) return null;
            return SongData;
        }
        public bool FameCheckAvailable(HashSet<long> CheckedLocations, out long FameCheckID)
        {
            if (!HasFameCheck(out FameCheckID)) return false;
            if (CheckedLocations.Contains(FameCheckID)) return false;
            bool standardComplete = !HasStandardCheck(out var sl) || CheckedLocations.Contains(sl);
            bool extraComplete = !HasExtraCheck(out var el) || CheckedLocations.Contains(el);
            return standardComplete && extraComplete;
        }
        public bool HasUncheckedLocations(HashSet<long> CheckedLocations)
        {
            if (HasStandardCheck(out var sl) && !CheckedLocations.Contains(sl))
                return true;
            if (HasExtraCheck(out var el) && !CheckedLocations.Contains(el))
                return true;
            if (HasFameCheck(out var fl) && !CheckedLocations.Contains(fl))
                return true;
            return false;
        }
    }
}
