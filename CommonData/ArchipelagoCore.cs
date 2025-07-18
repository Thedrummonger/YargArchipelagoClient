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
using static YargArchipelagoCommon.CommonData;
using static YargArchipelagoCommon.CommonData.Networking;

namespace YargArchipelagoPlugin
{
    public class ArchipelagoService
    {
        public ArchipelagoService(ManualLogSource LogSource)
        {
            packetClient = new YargPacketClient(this);
            harmony = new Harmony(ArchipelagoPlugin.pluginGuid);
            Logger = LogSource;
        }
        private ManualLogSource Logger;
        private Harmony harmony;
        public Harmony GetPatcher() => harmony;
        private GameManager CurrentGame = null;
        public string[] CurrentlyAvailableSongs { get; private set; } = Array.Empty<string>();

        public bool HasAvailableSongUpdate { get; set; } = false;
        public YargPacketClient packetClient { get; private set; }

        public void SetCurrentSong(GameManager Manager) => CurrentGame = Manager;
        public void ClearCurrentSong() => CurrentGame = null;
        public bool IsInSong() => CurrentGame != null;
        public GameManager GetCurrentSong() => CurrentGame;
        public void Log(string message, LogLevel level = LogLevel.Info) => Logger.Log(level, message);

        public void StartAPPacketServer()
        {
            _ = packetClient.ConnectAsync();
            packetClient.DeathLinkReceived += deathLinkData => YargEngineActions.ForceExitSong(this);
            packetClient.AvailableSongsReceived += AvailableSongs => {
                UpdateCurrentlyAvailable(AvailableSongs);
                if (!IsInSong() || !YargEngineActions.UpdateRecommendedSongsMenu())
                    HasAvailableSongUpdate = true;
            };
            packetClient.ActionItemReceived += item => { YargEngineActions.ApplyActionItem(this, item); };
        }

        public void UpdateCurrentlyAvailable(IEnumerable<string> availableSongs)
        {
            CurrentlyAvailableSongs = availableSongs.ToArray();
            HasAvailableSongUpdate = true;
        }

        public SongEntry[] GetAvailableSongs()
        {
            List<SongEntry> songEntries = new List<SongEntry>();
            foreach (var i in CurrentlyAvailableSongs)
            {
                var Target = SongContainer.Songs.FirstOrDefault(x => Convert.ToBase64String(x.Hash.HashBytes) == i);
                if (Target != null)
                    songEntries.Add(Target);
            }
            Log($"{songEntries.Count} Songs Available");
            return songEntries.ToArray();
        }
    }

    public class ArchipelagoEventManager
    {
        public ArchipelagoService APHandler { get; private set; }
        public ArchipelagoEventManager(ArchipelagoService Handler)
        {
            APHandler = Handler;
        }
        public void SendSongCompletionResults(List<PlayerScoreRecord> playerScoreRecords, GameRecord record)
        {
            APHandler.Log($"Recording Score for AP");
            var songPassInfo = new CommonData.SongPassInfo(Convert.ToBase64String(record.SongChecksum));
            songPassInfo.participants = playerScoreRecords
                .Where(x => YargAPUtils.IsSupportedInstrument(x.Instrument, out _))
                .Select(x => new CommonData.SongParticipantInfo()
                {
                    Difficulty = YargAPUtils.GetSupportedDifficulty(x.Difficulty),
                    instrument = YargAPUtils.IsSupportedInstrument(x.Instrument, out var SupportedInstrument) ? SupportedInstrument : null,
                    FC = x.IsFc,
#if NIGHTLY
                    Percentage = x.Percent ?? 0,
#elif STABLE
                    Percentage = x.Percent,
#endif
                    Score = x.Score,
                    Stars = StarAmountHelper.GetStarCount(x.Stars),
                    WasGoldStar = x.Stars == StarAmount.StarGold,
                }).ToArray();
            songPassInfo.SongPassed = true; //Always true for now, will be handled when yarg implements fail mode

            _ = APHandler.packetClient?.SendPacketAsync(new YargAPPacket { passInfo = songPassInfo });
        }

        public void SongStarted(GameManager gameManager)
        {
            APHandler.SetCurrentSong(gameManager);
            _ = APHandler.packetClient?.SendPacketAsync(new YargAPPacket
            {
                CurrentlyPlaying = CommonData.CurrentlyPlayingData.CurrentlyPlayingSong(gameManager.Song.ToSongData())
            });
        }
        public void SongEnded()
        {
            APHandler.ClearCurrentSong();
            APHandler.HasAvailableSongUpdate = true;
            _ = APHandler.packetClient?.SendPacketAsync(new YargAPPacket
            {
                CurrentlyPlaying = CommonData.CurrentlyPlayingData.CurrentlyPlayingNone()
            });
        }
    }

    public static class YargAPUtils
    {
        public static bool IsSupportedInstrument(Instrument source, out CommonData.SupportedInstrument? target)
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
        public static CommonData.SupportedDifficulty GetSupportedDifficulty(Difficulty source)
        {
            if (source > Difficulty.Expert)
                return CommonData.SupportedDifficulty.Expert;
            if (source < Difficulty.Easy)
                return CommonData.SupportedDifficulty.Easy;
            return (CommonData.SupportedDifficulty)(int)source;
        }
        public static CommonData.SongData ToSongData(this SongEntry song)
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
    }

    public static class YargEngineActions
    {
        public static void ApplyActionItem(ArchipelagoService APHandler, ActionItemData ActionItem)
        {
            if (ActionItem.type == CommonData.FillerTrapType.Restart)
                ForceExitSong(APHandler);
            if (ActionItem.type == CommonData.FillerTrapType.StarPower && !APHandler.IsInSong() && !APHandler.GetCurrentSong().IsPractice)
                foreach (var i in APHandler.GetCurrentSong().Players)
                    ApplyStarPowerItem(i, APHandler);
        }
        public static void ApplyStarPowerItem(BasePlayer player, ArchipelagoService handler)
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
                handler.Log($"Failed to apply start power to engine of type {engine.GetType()}\n{e}");
            }
#endif
        }

        public static void ForceExitSong(ArchipelagoService handler)
        {
            if (!handler.IsInSong())
                return;
            try
            {
                handler.GetCurrentSong().ForceQuitSong();
            }
            catch (Exception e)
            {
                handler.Log($"Failed to apply deathlink\n{e}");
            }
        }
        public static bool UpdateRecommendedSongsMenu()
        {
            var Menu = UnityEngine.Object.FindObjectOfType<MusicLibraryMenu>();
            if (Menu == null || !Menu.gameObject.activeInHierarchy)
                return false;

            Menu.RefreshAndReselect();
            return true;
        }
        public static void DumpAvailableSongs(SongCache SongCache, ArchipelagoService handler)
        {
            Dictionary<string, CommonData.SongData> SongData = new Dictionary<string, CommonData.SongData>();

            foreach (var instrument in SongCache.Instruments)
            {
                if (!YargAPUtils.IsSupportedInstrument(instrument.Key, out var supportedInstrument))
                    continue;
                foreach (var Difficulty in instrument.Value)
                {
                    if (Difficulty.Key < 0)
                        continue;
                    foreach (var song in Difficulty.Value)
                    {
                        var data = YargAPUtils.ToSongData(song);
                        if (!SongData.ContainsKey(data.SongChecksum))
                            SongData[data.SongChecksum] = data;
                        SongData[data.SongChecksum].Difficulties[supportedInstrument.Value] = Difficulty.Key;
                    }
                }
            }
            handler.Log($"Dumping Info for {SongData.Values.Count} songs");
            if (!Directory.Exists(CommonData.DataFolder)) Directory.CreateDirectory(CommonData.DataFolder);
            File.WriteAllText(CommonData.SongExportFile, JsonConvert.SerializeObject(SongData.Values.ToArray(), Formatting.Indented));
        }
    }
}
