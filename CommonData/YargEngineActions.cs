using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

//Don't Let visual studios lie to me these are needed
using YARG.Core.Engine;
using YARG.Core.Song;
using YARG.Core.Song.Cache;
using YARG.Gameplay;
using YARG.Core.Audio;
using YARG.Menu.Persistent;

//----------------------------------------------------
using YARG.Gameplay.Player;
using YARG.Menu.MusicLibrary;
using YargArchipelagoCommon;
using YARG.Gameplay.HUD;
using Cysharp.Threading.Tasks;

namespace YargArchipelagoPlugin
{
    public static class YargEngineActions
    {
        public static void ApplyActionItem(ArchipelagoService APHandler, CommonData.ActionItemData ActionItem)
        {
            APHandler.Log($"Applying Action Item {ActionItem.type}");
            if (!APHandler.IsInSong() || APHandler.GetCurrentSong().IsPractice)
            {
                APHandler.Log($"Exiting, not in Song");
                return;
            }

            switch (ActionItem.type)
            {
                case CommonData.APActionItem.Restart:
                    ApplyRestartTrap(APHandler);
                    break;
                case CommonData.APActionItem.StarPower:
                    foreach (var i in APHandler.GetCurrentSong().Players)
                        ApplyStarPowerItem(i, APHandler);
                    break;
            }
        }
        public static void ApplyStarPowerItem(BasePlayer player, ArchipelagoService handler)
        {
            if (!handler.IsInSong())
                return;
            handler.Log($"Gaining Star Power");
            MethodInfo method = AccessTools.Method(typeof(BaseEngine), "GainStarPower");
            method.Invoke(player.BaseEngine, new object[] { player.BaseEngine.TicksPerQuarterSpBar });

        }

        public static void ApplyDeathLink(ArchipelagoService handler, CommonData.DeathLinkData deathLinkData)
        {
            if (!handler.IsInSong())
                return;
            try
            {
                handler.Log($"Applying Death Link");
#if STABLE
                ForceExitSong(handler);
#else
                ForceFailSong(handler);
#endif
                ToastManager.ToastInformation($"DeathLink Received!\n\n{deathLinkData.Source} {deathLinkData.Cause}");
                //DialogManager.Instance.ShowMessage("DeathLink Received!", $"{deathLinkData.Source} {deathLinkData.Cause}");
            }
            catch (Exception e)
            {
                handler.Log($"Failed to apply deathlink\n{e}");
            }
        }

        public static void ApplyRestartTrap(ArchipelagoService handler)
        {
            ForceRestartSong(handler);
            ToastManager.ToastInformation("A player has sent you a Restart Trap!");
            //DialogManager.Instance.ShowMessage("Restart Trap","A player has sent you a Restart Trap!");
        }

        private static void ForceRestartSong(ArchipelagoService handler)
        {
            if (!handler.IsInSong()) 
                return;
            try
            {
                var gm = handler.GetCurrentSong();
                var field = AccessTools.Field(typeof(GameManager), "_pauseMenu");
                object pauseMenuObj = field.GetValue(gm);
                if (pauseMenuObj is PauseMenuManager pm)
                {
                    //TODO: This works but YARG spits out a bunch of errors. I thinks it because I don't give the pause menu enough time to load before restarting.
                    if (!gm.Paused)
                        gm.Pause(true);
                    pm.Restart();
                }
            }
            catch (Exception e)
            {
                handler.Log($"Failed to force restart song\n{e}");
            }
        }

#if NIGHTLY
        public static async void ForceFailSong(ArchipelagoService handler)
        {
            var gameManager = handler.GetCurrentSong();
            if (!handler.IsInSong() || gameManager.IsPractice)
                return;

            gameManager.PlayerHasFailed = true;
            try
            {
                var mixerObj = AccessTools.Field(typeof(GameManager), "_mixer")?.GetValue(gameManager);
                var fade = mixerObj?.GetType().GetMethod("FadeOut", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                fade?.Invoke(mixerObj, new object[] { GameManager.SONG_END_DELAY });
            }
            catch { }
            await UniTask.Delay(TimeSpan.FromSeconds(GameManager.SONG_END_DELAY));
            GlobalAudioHandler.PlayVoxSample(VoxSample.FailSound);
            gameManager.Pause(true);
        }
#endif

        private static void ForceExitSong(ArchipelagoService handler)
        {
            if (!handler.IsInSong())
                return;
            try
            {
                handler.Log($"Forcing Quit");
                handler.GetCurrentSong().ForceQuitSong();
            }
            catch (Exception e)
            {
                handler.Log($"Failed to force exit song\n{e}");
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
