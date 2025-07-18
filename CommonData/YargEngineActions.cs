using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
//Don't Let visual studios like to me these are needed
using YARG.Core.Engine;
using YARG.Core.Song;
using YARG.Core.Song.Cache;
//----------------------------------------------------
using YARG.Gameplay.Player;
using YARG.Menu.MusicLibrary;
using YargArchipelagoCommon;

namespace YargArchipelagoPlugin
{
    public static class YargEngineActions
    {
        public static void ApplyActionItem(ArchipelagoService APHandler, CommonData.ActionItemData ActionItem)
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
