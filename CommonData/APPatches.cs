using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using YARG.Core.Chart;
using YARG.Core.Engine;
using YARG.Core.Engine.Drums;
using YARG.Core.Engine.Guitar;
using YARG.Core.Engine.Vocals;
using YARG.Core.Logging;
using YARG.Core.Song;
using YARG.Core.Song.Cache;
using YARG.Gameplay;
using YARG.Gameplay.Player;
using YARG.Scores;

namespace YargArchipelagoPlugin
{
    class APPatches
    {
        public static void GameManager_Awake(GameManager __instance)
        {
            Archipelago.SongStarted(__instance);
        }
        public static void GameManager_OnDestroy()
        {
            Archipelago.SongEnded();
        }
        public static void ScoreContainer_RecordScore(GameRecord gameRecord, List<PlayerScoreRecord> playerEntries)
        {
            Archipelago.RecordScoreForArchipelago(playerEntries, gameRecord);
        }
        public static void SongContainer_FillContainers(SongCache ____songCache)
        {
            Archipelago.DumpAvailableSongs(____songCache);
        }
        public static bool RecommendedSongs_GetRecommendedSongs(ref SongEntry[] __result)
        {
            __result = Archipelago.GetAvailableSongs();
            return false;
        }
        public static void ApplyStarPowerItem(BasePlayer player)
        {
#if NIGHTLY
            MethodInfo method = AccessTools.Method(typeof(BaseEngine), "GainStarPower");
            method.Invoke(player.BaseEngine, new object[] { player.BaseEngine.TicksPerQuarterSpBar });
#elif STABLE
            if (player.BaseEngine is GuitarEngine guitarEngine)
            {
                var NewAmount = guitarEngine.EngineStats.StarPowerAmount + 0.25;
                if (NewAmount > 1) NewAmount = 1;
                guitarEngine.EngineStats.StarPowerAmount = NewAmount;
                MethodInfo rebaseProgressValuesMethod = AccessTools.Method(typeof(GuitarEngine), "RebaseProgressValues");
                rebaseProgressValuesMethod.Invoke(guitarEngine, new object[] { guitarEngine.State.CurrentTick });
            }
            else if (player.BaseEngine is DrumsEngine drumsEngine)
            {
                var NewAmount = drumsEngine.EngineStats.StarPowerAmount + 0.25;
                if (NewAmount > 1) NewAmount = 1;
                drumsEngine.EngineStats.StarPowerAmount = NewAmount;
                MethodInfo rebaseProgressValuesMethod = AccessTools.Method(typeof(DrumsEngine), "RebaseProgressValues");
                rebaseProgressValuesMethod.Invoke(drumsEngine, new object[] { drumsEngine.State.CurrentTick });
            }
            else if (player.BaseEngine is VocalsEngine vocalsEngine)
            {
                var NewAmount = vocalsEngine.EngineStats.StarPowerAmount + 0.25;
                if (NewAmount > 1) NewAmount = 1;
                vocalsEngine.EngineStats.StarPowerAmount = NewAmount;
                MethodInfo rebaseProgressValuesMethod = AccessTools.Method(typeof(VocalsEngine), "RebaseProgressValues");
                rebaseProgressValuesMethod.Invoke(vocalsEngine, new object[] { vocalsEngine.State.CurrentTick });
            }
#endif
        }
    }
}
