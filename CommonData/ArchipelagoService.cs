using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using YARG.Core.Song;
using YARG.Gameplay;
using YARG.Song;

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
                if (IsInSong() || !YargEngineActions.UpdateRecommendedSongsMenu())
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
}
