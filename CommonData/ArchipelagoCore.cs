using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YARG.Core;
using YARG.Core.Engine;
using YARG.Core.Game;
using YARG.Core.Song;
using YARG.Core.Song.Cache;
using YARG.Core.Utility;
using YARG.Gameplay;
using YARG.Gameplay.Player;
using YARG.Menu.MusicLibrary;
using YARG.Scores;
using YARG.Song;
using YargArchipelagoCommon;
using static YargArchipelagoCommon.CommonData.Networking;

namespace YargArchipelagoPlugin
{
    public class Archipelago
    {
        public static YargPacketClient packetClient;
        public static Action<BasePlayer> _gainStarPowerDelegate;
        public static ManualLogSource ManualLogSource;
        private static GameManager CurrentGame = null;
        public static bool HasAvailableSongUpdate = false;

        public static string[] CurrentlyAvailableSongs = Array.Empty<string>();
        public static void RecordScoreForArchipelago(List<PlayerScoreRecord> playerScoreRecords, GameRecord record)
        {
            ManualLogSource?.LogInfo($"Recording Score for AP");
            var songPassInfo = new CommonData.SongPassInfo(Convert.ToBase64String(record.SongChecksum));
            songPassInfo.participants = playerScoreRecords
                .Where(x => IsSupportedInstrument(x.Instrument, out _))
                .Select(x => new CommonData.SongParticipantInfo()
                {
                    Difficulty = GetSupportedDifficulty(x.Difficulty),
                    instrument = IsSupportedInstrument(x.Instrument, out var SupportedInstrument) ? SupportedInstrument : null,
                    FC = x.IsFc,
#if NIGHTLY
                    Percentage = x.Percent??0,
#elif STABLE
                    Percentage = x.Percent,
#endif
                    Score = x.Score,
                    Stars = StarAmountHelper.GetStarCount(x.Stars),
                    WasGoldStar = x.Stars == StarAmount.StarGold,
                }).ToArray();
            songPassInfo.SongPassed = true; //Always true for now, will be handled when yarg implements fail mode

            _ = packetClient?.SendPacketAsync(new YargAPPacket { passInfo = songPassInfo });
        }

        public static void SongStarted(GameManager gameManager)
        {
            CurrentGame = gameManager;
            _ = packetClient?.SendPacketAsync(new YargAPPacket
            {
                CurrentlyPlaying = CommonData.CurrentlyPlayingData.CurrentlyPlayingSong(ToSongData(gameManager.Song))
            });
        }
        public static void SongEnded()
        {
            CurrentGame = null;
            HasAvailableSongUpdate = true;
            _ = packetClient?.SendPacketAsync(new YargAPPacket
            {
                CurrentlyPlaying = CommonData.CurrentlyPlayingData.CurrentlyPlayingNone()
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
                        SongData[data.SongChecksum].Difficulties[supportedInstrument.Value] = Difficulty.Key;
                    }
                }
            }
            ManualLogSource?.LogInfo($"Dumping Info for {SongData.Values.Count} songs");
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
            ManualLogSource?.LogInfo($"{songEntries.Count} Songs Available");
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
#if NIGHTLY
                Path = song.ActualLocation,
#elif STABLE
                Path = song.Directory,
#endif
                SongChecksum = Convert.ToBase64String(song.Hash.HashBytes),
                Difficulties = new Dictionary<CommonData.SupportedInstrument, int>()
            };
        }
        private static bool IsSupportedInstrument(Instrument source, out CommonData.SupportedInstrument? target)
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
            YargAPPacket BasePacket;
            try
            {
                BasePacket = JsonConvert.DeserializeObject<YargAPPacket>(line, PacketSerializeSettings);
            }
            catch (Exception e)
            {
                ManualLogSource?.LogInfo($"Failed to parse client packet\n{line}\n{e}");
                return;
            }

            if (BasePacket.deathLinkData != null)
                CauseDeathLink();
            if (BasePacket.trapData != null)
            {
                if (BasePacket.trapData.type == CommonData.FillerTrapType.Restart)
                    CauseDeathLink();
                if (BasePacket.trapData.type == CommonData.FillerTrapType.StarPower && CurrentGame != null && !CurrentGame.IsPractice)
                    foreach (var i in CurrentGame.Players)
                        ApplyStarPowerItem(i);
            }
            if (BasePacket.AvailableSongs != null)
            {
                CurrentlyAvailableSongs = BasePacket.AvailableSongs;
                if (CurrentGame != null || !UpdateRecommendedSongsMenu())
                    HasAvailableSongUpdate = true;
            }
        }

        private static void CauseDeathLink()
        {
            try
            {
                CurrentGame?.ForceQuitSong();
            }
            catch (Exception e)
            {
                ManualLogSource?.LogInfo($"Failed to apply deathlink\n{e}");
            }
        }

        public static void ApplyStarPowerItem(BasePlayer player)
        {
#if NIGHTLY
            // thank you nightly build for being cool and letting me call GainStarPower directly from BaseEngine
            MethodInfo method = AccessTools.Method(typeof(BaseEngine), "GainStarPower");
            method.Invoke(player.BaseEngine, new object[] { player.BaseEngine.TicksPerQuarterSpBar });
#elif STABLE
            var engine = player.BaseEngine;
            try
            {
                // stable build is not cool
                dynamic stats = AccessTools.Field(engine.GetType(), "EngineStats").GetValue(engine);
                double newAmount = stats.StarPowerAmount + 0.25;
                stats.StarPowerAmount = (newAmount > 1) ? 1 : newAmount;

                MethodInfo rebase = AccessTools.Method(engine.GetType(), "RebaseProgressValues");
                dynamic state = AccessTools.Property(engine.GetType(), "State").GetValue(engine);
                rebase.Invoke(engine, new object[] { state.CurrentTick });
            }
            catch (Exception e)
            {
                ManualLogSource?.LogInfo($"Failed to apply start power to engine of type {engine.GetType()}\n{e}");
            }
#endif
        }

        public static bool UpdateRecommendedSongsMenu()
        {
            var Menu = UnityEngine.Object.FindObjectOfType<MusicLibraryMenu>();
            if (Menu == null || !Menu.gameObject.activeInHierarchy)
                return false;

            Menu.RefreshAndReselect();
            return true;
        }
    }
}
