using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using YARG.Core.Engine;
using YARG.Core.Song;
using YARG.Core.Song.Cache;
using YARG.Gameplay;
using YARG.Gameplay.Player;
using YARG.Menu.MusicLibrary;
using YARG.Scores;
using YARG.Song;

namespace YargArchipelagoPlugin
{
    [HarmonyPatch]
    public static class APPatches
    {
        #region GameManager

        [HarmonyPatch(typeof(GameManager), "Awake")]
        [HarmonyPostfix]
        public static void GameManager_Awake(GameManager __instance) => Archipelago.SongStarted(__instance);

        [HarmonyPatch(typeof(GameManager), "OnDestroy")]
        [HarmonyPrefix]
        public static void GameManager_OnDestroy() => Archipelago.SongEnded();

        #endregion

        #region ScoreContainer

        [HarmonyPatch(typeof(ScoreContainer), "RecordScore")]
        [HarmonyPostfix]
        public static void ScoreContainer_RecordScore(GameRecord gameRecord, List<PlayerScoreRecord> playerEntries)
            => Archipelago.RecordScoreForArchipelago(playerEntries, gameRecord);

        #endregion

        #region SongContainer

        [HarmonyPatch(typeof(SongContainer), "FillContainers")]
        [HarmonyPostfix]
        public static void SongContainer_FillContainers(SongCache ____songCache)
            => Archipelago.DumpAvailableSongs(____songCache);

        #endregion

        #region RecommendedSongs

        [HarmonyPatch(typeof(RecommendedSongs), "GetRecommendedSongs")]
        [HarmonyPrefix]
        public static bool RecommendedSongs_GetRecommendedSongs(ref SongEntry[] __result)
        {
            __result = Archipelago.GetAvailableSongs();
            return false;
        }

        #endregion

        #region MusicLibraryMenu

        [HarmonyPatch(typeof(MusicLibraryMenu), "OnEnable")]
        [HarmonyPostfix]
        public static void MusicLibraryMenu_OnEnable(MusicLibraryMenu __instance)
        {
            if (Archipelago.HasAvailableSongUpdate)
            {
                MethodInfo m = AccessTools.Method(typeof(MusicLibraryMenu), "SetRecommendedSongs");
                m.Invoke(__instance, null);
            }
        }

        [HarmonyPatch(typeof(MusicLibraryMenu), "SetRecommendedSongs")]
        [HarmonyPostfix]
        public static void MusicLibraryMenu_SetRecommendedSongs()
            => Archipelago.HasAvailableSongUpdate = false;

        #endregion
    }
}
