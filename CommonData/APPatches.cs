using System.Collections.Generic;
using YARG.Core.Song;
using YARG.Core.Song.Cache;
using YARG.Gameplay;
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
    }
}
