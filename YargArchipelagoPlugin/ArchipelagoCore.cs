using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YARG.Gameplay;
using YARG.Scores;
using YARG.Core.Song;
using YARG.Core;
using Newtonsoft.Json;
using YARG.Core.Utility;
using YARG.Core.Game;
using YARG.Song;
using YargArchipelagoCommon;
using static YargArchipelagoCommon.CommonData.Networking;

namespace YargArchipelagoPlugin
{
    public class Archipelago
    {
        public static YargPacketClient packetClient;

        private static GameManager CurrentGame = null;
        public static string[] CurrentlyAvailableSongs = Array.Empty<string>();
        public static void RecordScoreForArchipelago(List<PlayerScoreRecord> playerScoreRecords, GameRecord record)
        {
            ArchipelagoPlugin.ManualLogSource?.LogInfo($"Recording Score for AP");
            var songPassInfo = new CommonData.SongPassInfo(Convert.ToBase64String(record.SongChecksum));
            songPassInfo.participants = playerScoreRecords
                .Where(x => IsSupportedInstrument(x.Instrument, out _))
                .Select(x => new CommonData.SongParticipantInfo()
                {
                    Difficulty = GetSupportedDifficulty(x.Difficulty),
                    instrument = IsSupportedInstrument(x.Instrument, out var SupportedInstrument) ? SupportedInstrument : default,
                    FC = x.IsFc,
                    Percentage = x.Percent.Value,
                    Score = x.Score,
                    Stars = StarAmountHelper.GetStarCount(x.Stars),
                    WasGoldStar = x.Stars == StarAmount.StarGold,
                }).ToArray();
            songPassInfo.SongPassed = true; //Always true for now, will be handled when yarg implements fail mode

            _ = packetClient?.SendPacketAsync(new YargDataPacket { passInfo = songPassInfo });
        }

        public static void SongStarted(GameManager gameManager)
        {
            CurrentGame = gameManager;
            _ = packetClient?.SendPacketAsync(new YargDataPacket
            {
                CurrentlyPlaying = Convert.ToBase64String(gameManager.Song.Hash.HashBytes)
            });
        }
        public static void SongEnded()
        {
            CurrentGame = null;
            _ = packetClient?.SendPacketAsync(new YargDataPacket
            {
                CurrentlyPlaying = string.Empty
            });
        }

        public static void DumpAvailableSongs(SongCache SongCache)
        {
            Dictionary<string, CommonData.SongData> SongData = new Dictionary<string, CommonData.SongData>();

            foreach (var instrument in SongCache.Instruments)
            {
                if (!IsSupportedInstrument(instrument.Key, out var supportedInstrument))
                    continue;
                foreach (var Difficulty in instrument.Value)
                {
                    if (Difficulty.Key < 0)
                        continue;
                    foreach (var song in Difficulty.Value)
                    {
                        var data = ToSongData(song);
                        if (!SongData.ContainsKey(data.SongChecksum))
                            SongData[data.SongChecksum] = data;
                        SongData[data.SongChecksum].Difficulties[supportedInstrument] = Difficulty.Key;
                    }
                }
            }
            ArchipelagoPlugin.ManualLogSource?.LogInfo($"Dumping Info for {SongData.Values.Count} songs");
            if (!Directory.Exists(CommonData.DataFolder)) Directory.CreateDirectory(CommonData.DataFolder);
            File.WriteAllText(CommonData.SongExportFile, JsonConvert.SerializeObject(SongData.Values.ToArray(), Formatting.Indented));
        }

        public static SongEntry[] GetAvailableSongs()
        {
            List<SongEntry> songEntries = new List<SongEntry>();
            foreach (var i in CurrentlyAvailableSongs)
            {
                var Target = SongContainer.Songs.FirstOrDefault(x => Convert.ToBase64String(x.Hash.HashBytes) == i);
                if (Target != null)
                    songEntries.Add(Target);
            }
            ArchipelagoPlugin.ManualLogSource?.LogInfo($"{songEntries.Count} Songs Available");
            return songEntries.ToArray();
        }

        private static CommonData.SongData ToSongData(SongEntry song)
        {
            return new CommonData.SongData()
            {
                Album = RichTextUtils.StripRichTextTags(song.Album),
                Artist = RichTextUtils.StripRichTextTags(song.Artist),
                Charter = RichTextUtils.StripRichTextTags(song.Charter),
                Name = RichTextUtils.StripRichTextTags(song.Name),
                Path = song.ActualLocation,
                SongChecksum = Convert.ToBase64String(song.Hash.HashBytes),
                Difficulties = new Dictionary<CommonData.SupportedInstrument, int>()
            };
        }
        private static bool IsSupportedInstrument(Instrument source, out CommonData.SupportedInstrument target)
        {
            int value = (int)source;
            if (Enum.IsDefined(typeof(CommonData.SupportedInstrument), value))
            {
                target = (CommonData.SupportedInstrument)value;
                return true;
            }
            target = default;
            return false;
        }
        private static CommonData.SupportedDifficulty GetSupportedDifficulty(Difficulty source)
        {
            if (source > Difficulty.Expert)
                return CommonData.SupportedDifficulty.Expert;
            if (source < Difficulty.Easy)
                return CommonData.SupportedDifficulty.Easy;
            return (CommonData.SupportedDifficulty)(int)source;
        }

        public static void StartAPClient()
        {
            packetClient = new YargPacketClient();
            _ = packetClient.ConnectAsync();
        }

        internal static void ParseClientPacket(string line)
        {
            ClientDataPacket BasePacket;
            try
            {
                BasePacket = JsonConvert.DeserializeObject<ClientDataPacket>(line);
            }
            catch (Exception e)
            {
                ArchipelagoPlugin.ManualLogSource?.LogInfo($"Failed to parse client packet\n{line}\n{e}");
                return;
            }

            if (BasePacket.deathLinkData != null)
                CauseDeathLink();
            if (BasePacket.trapData != null)
            {
                if (BasePacket.trapData.type == CommonData.trapType.Restart)
                    CauseDeathLink();
            }
            if (BasePacket.AvailableSongs != null)
                CurrentlyAvailableSongs = BasePacket.AvailableSongs;
        }

        private static void CauseDeathLink()
        {
            try
            {
                CurrentGame?.ForceQuitSong();
            }
            catch (Exception e)
            {
                ArchipelagoPlugin.ManualLogSource?.LogInfo($"Failed to apply deathlink\n{e}");
            }
        }
    }
}
