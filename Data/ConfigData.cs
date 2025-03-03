using Archipelago.MultiClient.Net;
using static YargArchipelagoClient.Data.Constants;

namespace YargArchipelagoClient.Data
{
    public class ConfigData()
    {
        /// <summary>
        /// A list of songs and their data pulled from the given song folder
        /// </summary>
        public Dictionary<string, SongData> SongData { get; set; } = [];
        /// <summary>
        /// A mapping of the song "number" to it's AP data and mapped song
        /// </summary>
        public Dictionary<int, SongLocation> ApLocationData { get; } = [];
        /// <summary>
        /// The goal song info
        /// </summary>
        public SongLocation GoalSong = new(0);

        public bool BroadcastSongName = false;

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
        public string? MappedSong = null;
        public long? APStandardCheckLocation = null;
        public long? APExtraCheckLocation = null;
        public long? APFameCheckLocation = null;
        public SongProfile? Requirements = null;
        public string DisplayName =>
            $"{MappedSong} [{Requirements?.Name}]";
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
        public SongData? GetSongData(ConfigData config)
        {
            if (config is null) return null;
            if (MappedSong is null) return null;
            if (!config.SongData.TryGetValue(MappedSong, out var SongData)) return null;
            return SongData;
        }
        public bool FameCheckAvailable(HashSet<long> CheckedLocations, out long FameCheckID)
        {
            FameCheckID = -1;
            if (!HasFameCheck(out _)) return false;
            bool standardComplete = !HasStandardCheck(out var sl) || CheckedLocations.Contains(sl);
            bool extraComplete = !HasExtraCheck(out var el) || CheckedLocations.Contains(el);
            return standardComplete && extraComplete;
        }
    }

    // Custom object to store song data with helper methods.
    public class SongData(Dictionary<string, string> data)
    {
        public Dictionary<string, string> Data { get; private set; } = data;

        public string GetValueOrDefault(string key, string defaultValue = "") =>
            Data.TryGetValue(key, out var value) ? value : defaultValue;

        public int GetIntValueOrDefault(string key, int defaultValue = 0) =>
            Data.TryGetValue(key, out var value) && int.TryParse(value, out int result) ? result : defaultValue;

        public string Name => GetValueOrDefault("name");
        public string Artist => GetValueOrDefault("artist");
        public string Album => GetValueOrDefault("album");
        public string Genre => GetValueOrDefault("genre");
        public int Year => GetIntValueOrDefault("year");
        public string Charter => GetValueOrDefault("charter");
        public int SongLength => GetIntValueOrDefault("song_length");
        public string Icon => GetValueOrDefault("icon");

        /// <summary>
        /// Attempts to get a difficulty level for a given instrument.
        /// For example, calling TryGetDifficulty("guitar", out int diff) will attempt to get the value from "diff_guitar".
        /// </summary>
        public bool TryGetDifficulty(string instrument, out int difficulty)
        {
            difficulty = 0;
            string key = $"diff_{instrument.ToLower()}";
            return Data.TryGetValue(key, out var value) && int.TryParse(value, out difficulty);
        }
        /// <summary>
        /// Attempts to get a difficulty level for a given instrument.
        /// For example, calling TryGetDifficulty(Instrument.Guitar, out int diff) will attempt to get the value from "diff_guitar".
        /// </summary>
        public bool TryGetDifficulty(Instrument instrument, out int difficulty) =>
            TryGetDifficulty(instrument.ToString().ToLower(), out difficulty);
    }
}
