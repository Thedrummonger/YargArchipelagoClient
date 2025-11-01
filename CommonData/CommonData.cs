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
        public static string DataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YARChipelago");
        public static string ConnectionCachePath => Path.Combine(DataFolder, "connection.json");
        public static string SongExportFile => Path.Combine(DataFolder, "SongExport.json");
        public static string SeedConfigPath => Path.Combine(DataFolder, "seeds");
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

        public class SongCompletedData
        {
            public SongCompletedData(SongData Song, bool Passed) { songData = Song; SongPassed = Passed; }
            public SongData songData;
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

        public enum ItemLog
        {
            None = 0,
            ToMe = 1,
            All = 2,
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
            public const string PipeName = "yarg_ap_pipe";
            public class YargAPPacket
            {
                //YARG PARSED
                /// <summary>
                /// Sent to Yarg when a deathlink happens in AP. Causes the current song to fail and exit.
                /// </summary>
                public DeathLinkData deathLinkData = null;
                /// <summary>
                /// Sent to Yarg when an item is recieved that causes yarg to perform an action, such as adding start power or triggering traps
                /// </summary>
                public ActionItemData ActionItem = null;
                /// <summary>
                /// Sent to Yarg to update the game with what songs are available
                /// </summary>
                public (string SongHash, string Profile)[] AvailableSongs = null;

                //AP Parsed
                /// <summary>
                /// Sent to AP Client when a song is completed including whether it passed or failed and what the score was.
                /// </summary>
                public SongCompletedData SongCompletedInfo = null;
                /// <summary>
                /// Sent to AP Client when the currently playing song is changed to update the client title
                /// </summary>
                public CurrentlyPlayingData CurrentlyPlaying = null;

                //DUAL PARSED
                /// <summary>
                /// A log Message. When sent to AP client, prints to the chat log. When sent to Yarg, prints to the Unity Log
                /// </summary>
                public string Message = null;
            }

            public readonly static JsonSerializerSettings PacketSerializeSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
            };
        }
    }
}
