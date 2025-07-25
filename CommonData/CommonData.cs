﻿using Newtonsoft.Json;
//Don't Let visual studios lie to me these are needed
using System;
using System.Collections.Generic;
using System.IO;
//----------------------------------------------------

namespace YargArchipelagoCommon
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
            public Dictionary<SupportedInstrument, int> Difficulties = new Dictionary<SupportedInstrument, int>();
            public bool TryGetDifficulty(SupportedInstrument instrument, out int Difficulty) => Difficulties.TryGetValue(instrument, out Difficulty);
        }

        public class SongPassInfo
        {
            public SongPassInfo(string Hash, bool Passed = true) { SongHash = Hash; SongPassed = Passed; }
            public string SongHash;
            public bool SongPassed;
            public SongParticipantInfo[] participants = Array.Empty<SongParticipantInfo>();
        }

        public class SongParticipantInfo
        {
            public SupportedInstrument? instrument;
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

        public class DeathLinkData
        {
            public string Source;

            public string Cause;
        }
        public class ActionItemData
        {
            public ActionItemData(APActionItem t) { type = t; }
            public APActionItem type;
        }
        public class CurrentlyPlayingData
        {
            public CurrentlyPlayingData(SongData t) { song = t; }
            public SongData song;
            public static CurrentlyPlayingData CurrentlyPlayingSong(SongData t) => new CurrentlyPlayingData(t);
            public static CurrentlyPlayingData CurrentlyPlayingNone() => new CurrentlyPlayingData(null);
        }
        public enum APActionItem
        {
            Restart,
            StarPower,
            NonFiller
        }

        public static class Networking
        {
            public const int PORT = 26569;

            public class YargAPPacket
            {
                public SongPassInfo passInfo = null;
                public string Message = null;
                public CurrentlyPlayingData CurrentlyPlaying = null;
                public DeathLinkData deathLinkData = null;
                public ActionItemData ActionItem = null;
                public (string SongHash, string Profile)[] AvailableSongs = null;
            }

            public readonly static JsonSerializerSettings PacketSerializeSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
            };
        }
    }
}
