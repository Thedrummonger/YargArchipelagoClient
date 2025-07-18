using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using YARG.Core.Engine;
using YARG.Gameplay;
using YARG.Menu.MusicLibrary;
using YARG.Scores;
using YARG.Song;

namespace YargArchipelagoPlugin
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ArchipelagoPlugin : BaseUnityPlugin
    {
        public const string pluginGuid = "thedrummonger.yarg.archipelago";
        public const string pluginVersion = "0.1.0.0";
#if NIGHTLY
        public const string pluginName = "YARG Nightly Archipelago Plugin";
#else
        public const string pluginName = "YARG Archipelago Plugin";
#endif

        public void Awake()
        {
            var APHandler = new ArchipelagoService(Logger);
            APPatches.EventManager = new ArchipelagoEventManager(APHandler);

            APHandler.GetPatcher().PatchAll();

            Logger.LogInfo("Starting AP");
            APHandler.StartAPPacketServer();
        }
    }
}
