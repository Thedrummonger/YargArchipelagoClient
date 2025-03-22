using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YARG.Core.Engine;
using YARG.Gameplay;
using YARG.Menu.MusicLibrary;
using YARG.Scores;
using YARG.Song;
using YargArchipelagoPlugin;

namespace YargArchipelagoPluginMain
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ArchipelagoPlugin : BaseUnityPlugin
    {
        public const string pluginGuid = "thedrummonger.yarg.archipelago";
        public const string pluginName = "Yarg Archipelago Plugin Stable";
        public const string pluginVersion = "0.0.0.1";

        public void Awake()
        {
            Archipelago.ManualLogSource = Logger;
            Logger.LogInfo("Starting AP");

            Harmony harmony = new Harmony(pluginGuid);

            MethodInfo OriginalGameManagerAwake = AccessTools.Method(typeof(GameManager), "Awake");
            MethodInfo PatchedGameManagerAwake = AccessTools.Method(typeof(APPatches), "GameManager_Awake");
            harmony.Patch(OriginalGameManagerAwake, null, new HarmonyMethod(PatchedGameManagerAwake));

            MethodInfo OriginalGameManagerOnDestroy = AccessTools.Method(typeof(GameManager), "OnDestroy");
            MethodInfo PatchedGameManagerOnDestroy = AccessTools.Method(typeof(APPatches), "GameManager_OnDestroy");
            harmony.Patch(OriginalGameManagerOnDestroy, new HarmonyMethod(PatchedGameManagerOnDestroy));

            MethodInfo OriginalScoreContainerRecordScore = AccessTools.Method(typeof(ScoreContainer), "RecordScore");
            MethodInfo PatchedScoreContainerRecordScore = AccessTools.Method(typeof(APPatches), "ScoreContainer_RecordScore");
            harmony.Patch(OriginalScoreContainerRecordScore, null, new HarmonyMethod(PatchedScoreContainerRecordScore));

            MethodInfo OriginalSongContainerFillContainers = AccessTools.Method(typeof(SongContainer), "FillContainers");
            MethodInfo PatchedSongContainerFillContainers = AccessTools.Method(typeof(APPatches), "SongContainer_FillContainers");
            harmony.Patch(OriginalSongContainerFillContainers, null, new HarmonyMethod(PatchedSongContainerFillContainers));

            MethodInfo OriginalRecommendedSongsGetRecommendedSongs = AccessTools.Method(typeof(RecommendedSongs), "GetRecommendedSongs");
            MethodInfo PatchedRecommendedSongsGetRecommendedSongs = AccessTools.Method(typeof(APPatches), "RecommendedSongs_GetRecommendedSongs");
            harmony.Patch(OriginalRecommendedSongsGetRecommendedSongs, new HarmonyMethod(PatchedRecommendedSongsGetRecommendedSongs));

            Archipelago.StartAPClient();
        }
    }
}
