using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace YargArchipelagoClient.Data
{
    public class CommonData
    {

        public static string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "YARC", "YARG", "Archipelago");
        public static string SongExportFile = Path.Combine(DataFolder, "SongExport.json");
        public static string LastPlayedSong = Path.Combine(DataFolder, "LastPlayed.json");
        public enum SupportedInstrument
        {
            // Instruments are reserved in multiples of 10
            // 0-9: 5-fret guitar\
            FiveFretGuitar = 0,
            FiveFretBass = 1,
            Keys = 4,

            // 10-19: 6-fret guitar
            SixFretGuitar = 10,
            SixFretBass = 11,

            // 20-29: Drums
            FourLaneDrums = 20,
            ProDrums = 21,
            FiveLaneDrums = 22,

            // 30-39: Pro instruments
            ProKeys = 34,

            // 40-49: Vocals
            Vocals = 40,
            Harmony = 41,
        }

        public class SongData
        {
            public string Name;
            public string Artist;
            public string Album;
            public string Charter;
            public string Path;
            public string SongChecksum;
            public Dictionary<SupportedInstrument, int> Difficulties = new();
            public bool TryGetDifficulty(SupportedInstrument instrument, out int Difficulty) => Difficulties.TryGetValue(instrument, out Difficulty);
        }

        public class SongPassInfo
        {
            public SongPassInfo(string Hash) { SongHash = Hash; }
            public string SongHash;
            public SongParticipantInfo[] participants = Array.Empty<SongParticipantInfo>();
        }

        public class SongParticipantInfo
        {
            public SupportedInstrument instrument;
            public SupportedDifficulty Difficulty;
            public int Stars;
            public bool WasGoldStar;
            public bool FC;
            public int Score;
            public float Percentage;
        }

        public enum SupportedDifficulty
        {
            Easy = 1,
            Medium = 2,
            Hard = 3,
            Expert = 4,
        }
        public enum CompletionReq
        {
            Clear,
            OneStar,
            TwoStar,
            ThreeStar,
            FourStar,
            FiveStar,
            GoldStar,
            FullCombo
        }
        public enum StaticItems
        {
            [Description("Victory")]
            Victory,
            [Description("Fame Point")]
            FamePoint,
            [Description("Swap Song (Random)")]
            SwapRandom,
            [Description("Swap Song (Pick)")]
            SwapPick,
            [Description("Restart Trap")]
            TrapRestart
        }
    }
}
