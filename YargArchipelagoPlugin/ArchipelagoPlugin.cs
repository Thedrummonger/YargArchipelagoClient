﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using YARG.Gameplay;
using YARG.Song;
using YARG.Scores;
using YARG.Menu.MusicLibrary;
using YARG.Menu.Main;

namespace YargArchipelagoPlugin
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ArchipelagoPlugin : BaseUnityPlugin
    {
        public const string pluginGuid = "thedrummonger.yarg.archipelago";
        public const string pluginName = "Yarg Archipelago Plugin";
        public const string pluginVersion = "0.0.0.1";

        public static ManualLogSource ManualLogSource;

        public void Awake()
        {
            ManualLogSource = Logger;
            Logger.LogInfo("Starting AP");

            Harmony harmony = new Harmony(pluginGuid);

            //MethodInfo OriginalMainMenuStart = AccessTools.Method(typeof(MainMenu), "Start");
            //MethodInfo PatchedMainMenuStart = AccessTools.Method(typeof(APPatches), "MainMenu_Start");
            //harmony.Patch(OriginalMainMenuStart, null, new HarmonyMethod(PatchedMainMenuStart));

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
