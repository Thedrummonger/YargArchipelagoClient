using Archipelago.MultiClient.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YargArchipelagoClient.Data.Constants;

namespace YargArchipelagoClient.Data
{
    public class ConfigData()
    {
        /// <summary>
        /// A list of songs and their data pulled from the users song folder
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
            foreach(var i in archipelagoSession.Locations.AllLocations)
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
    }

    // Custom object to store song data with helper methods.
    public class SongData(Dictionary<string, string> data)
    {
        public Dictionary<string, string> Data { get; private set; } = data;

        // Checks if the specified key exists.
        public bool HasKey(string key)
        {
            return Data.ContainsKey(key);
        }

        // Attempts to get the value associated with the specified key.
        public bool TryGetValue(string key, out string? value)
        {
            return Data.TryGetValue(key, out value);
        }

        // Returns the value for the specified key or the provided default if the key is missing.
        public string GetValueOrDefault(string key, string defaultValue = "")
        {
            return Data.TryGetValue(key, out var value) ? value : defaultValue;
        }

        // Attempts to parse the value associated with the key as an integer.
        public bool TryGetIntValue(string key, out int intValue)
        {
            intValue = 0;
            if (Data.TryGetValue(key, out var value))
            {
                return int.TryParse(value, out intValue);
            }
            return false;
        }

        // Returns the integer value for the specified key or the provided default if missing or unparseable.
        public int GetIntValueOrDefault(string key, int defaultValue = 0)
        {
            return Data.TryGetValue(key, out var value) && int.TryParse(value, out int result) ? result : defaultValue;
        }

        // Common property helpers based on the example ini file.
        public string Name => GetValueOrDefault("name");
        public string Artist => GetValueOrDefault("artist");
        public string Album => GetValueOrDefault("album");
        public string Genre => GetValueOrDefault("genre");
        public int Year => GetIntValueOrDefault("year");
        public string Charter => GetValueOrDefault("charter");
        public int SongLength => GetIntValueOrDefault("song_length");
        public string Icon => GetValueOrDefault("icon");

        /// <summary>
        /// Attempts to get a difficulty level for a given instrument by using a string key.
        /// For example, calling TryGetDifficulty("guitar", out int diff) will attempt to get the value from "diff_guitar".
        /// </summary>
        public bool TryGetDifficulty(string instrument, out int difficulty)
        {
            string key = $"diff_{instrument.ToLower()}";
            return TryGetIntValue(key, out difficulty);
        }

        /// <summary>
        /// Overloaded version that accepts an Instrument enum.
        /// </summary>
        public bool TryGetDifficulty(Instrument instrument, out int difficulty)
        {
            // Convert the enum to a lowercase string and prefix with "diff_"
            string key = $"diff_{instrument.ToString().ToLower()}";
            return TryGetIntValue(key, out difficulty);
        }
    }
}
