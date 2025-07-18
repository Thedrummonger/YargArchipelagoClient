using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using YARG.Core.Engine;
using YARG.Core.Song;
using YARG.Core.Song.Cache;
using YARG.Gameplay;
using YARG.Gameplay.Player;
using YARG.Localization;
using YARG.Menu.MusicLibrary;
using YARG.Scores;
using YARG.Song;

namespace YargArchipelagoPlugin
{
    [HarmonyPatch]
    public static class APPatches
    {
        public static ArchipelagoEventManager EventManager;
        #region GameManager

        [HarmonyPatch(typeof(GameManager), "Awake")]
        [HarmonyPostfix]
        public static void GameManager_Awake(GameManager __instance) => EventManager.SongStarted(__instance);

        [HarmonyPatch(typeof(GameManager), "OnDestroy")]
        [HarmonyPrefix]
        public static void GameManager_OnDestroy() => EventManager.SongEnded();

        #endregion

        #region ScoreContainer

        [HarmonyPatch(typeof(ScoreContainer), "RecordScore")]
        [HarmonyPostfix]
        public static void ScoreContainer_RecordScore(GameRecord gameRecord, List<PlayerScoreRecord> playerEntries)
            => EventManager.SendSongCompletionResults(playerEntries, gameRecord);

        #endregion

        #region SongContainer

        [HarmonyPatch(typeof(SongContainer), "FillContainers")]
        [HarmonyPostfix]
        public static void SongContainer_FillContainers(SongCache ____songCache)
            => YargEngineActions.DumpAvailableSongs(____songCache, EventManager.APHandler);

        #endregion

        #region RecommendedSongs

        [HarmonyPatch(typeof(RecommendedSongs), "GetRecommendedSongs")]
        [HarmonyPrefix]
        public static bool RecommendedSongs_GetRecommendedSongs(ref SongEntry[] __result)
        {
            __result = EventManager.APHandler.GetAvailableSongs();
            return false;
        }

        #endregion

        #region MusicLibraryMenu

        [HarmonyPatch(typeof(MusicLibraryMenu), "OnEnable")]
        [HarmonyPostfix]
        public static void MusicLibraryMenu_OnEnable(MusicLibraryMenu __instance)
        {
            if (EventManager.APHandler.HasAvailableSongUpdate)
            {
                MethodInfo m = AccessTools.Method(typeof(MusicLibraryMenu), "SetRecommendedSongs");
                m.Invoke(__instance, null);
            }
        }

        [HarmonyPatch(typeof(MusicLibraryMenu), "SetRecommendedSongs")]
        [HarmonyPostfix]
        public static void MusicLibraryMenu_SetRecommendedSongs()
            => EventManager.APHandler.HasAvailableSongUpdate = false;

        #endregion

        [HarmonyPatch(typeof(MusicLibraryMenu), "CreateNormalViewList")]
        [HarmonyPostfix]
        public static List<ViewType> MusicLibraryMenu_CreateNormalViewList_Postfix(List<ViewType> __result)
        {
            var singularKey = Localize.Key("Menu.MusicLibrary.RecommendedSongs", "Singular");
            var pluralKey = Localize.Key("Menu.MusicLibrary.RecommendedSongs", "Plural");
            const string newHeader = "Available Archipelago Songs";

            var primaryField = AccessTools.Field(typeof(CategoryViewType), "_primary");

            foreach (var vt in __result)
            {
                if (vt is CategoryViewType cat)
                {
                    string current = (string)primaryField.GetValue(cat);
                    if (current == singularKey || current == pluralKey)
                    {
                        primaryField.SetValue(cat, newHeader);
                    }
                }
            }

            return __result;
        }
    }
}
