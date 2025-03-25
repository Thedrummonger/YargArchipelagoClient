using Newtonsoft.Json;
using System.Diagnostics;
using TDMUtils;
using YargArchipelagoClient.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static YargArchipelagoClient.Forms.PlandoForm;
using static YargArchipelagoCommon.CommonData;

namespace YargArchipelagoClient.Helpers
{
    class ClientInitializationHelper
    {
        public static bool ConnectToServer(out ConnectionData? connection)
        {
            connection = null;
            var CForm = new ConnectionForm();
            var dialog = CForm.ShowDialog();
            if (dialog != DialogResult.OK)
                return false;

            connection = CForm.Connection;
            return CForm.Connection is not null &&
                CForm.Connection.GetSession() is not null &&
                CForm.Connection.GetSession()!.Socket.Connected;
        }

        public static bool GetConfig(ConnectionData Connection, out ConfigData? configData)
        {
            configData = null;
            var SeedDir = ConnectionData.GetSeedPath();
            if (!Directory.Exists(SeedDir))
                Directory.CreateDirectory(SeedDir);

            var ConfigFile = Directory.GetFiles(SeedDir).FirstOrDefault(file => Path.GetFileName(file) == Connection.getSaveFileName());
            if (ConfigFile is not null)
            {
                Debug.WriteLine($"Seed Found {ConfigFile}");
                try { configData = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(ConfigFile)); }
                catch { configData = null; }
            }
            if (configData is null)
            {
                var configForm = new ConfigForm(Connection!);
                var Dialog = configForm.ShowDialog();
                if (Dialog != DialogResult.OK)
                    return false;
                configData = configForm.data!;
            }
            return configData is not null;
        }

        public static void ReadSlotData(ConnectionData Connection, ConfigData config)
        {

            var SlotData = Connection!.GetSession().DataStorage.GetSlotData();

            if (SlotData["fame_points_for_goal"] is Int64 FPSlotDataVal)
                config.FamePointsNeeded = (int)FPSlotDataVal;
            else
                throw new Exception("Could not get Fame Point Goal");

            if (SlotData.TryGetValue("death_link", out var DLO) && DLO is Int64 DLI && DLI > 0)
            {
                config.deathLinkEnabled = true;
                Connection.DeathLinkService?.EnableDeathLink();
            }
        }

        public static void AssignSongs(ConfigData data, ConnectionData Connection, List<SongPool> Pools, Dictionary<int, PlandoData> PlandoSongData, SongPoolManager songPoolManager)
        {
            Dictionary<string, HashSet<string>> UsedSongs = [];

            foreach (var song in PlandoSongData.Values.Where(x => x.PoolPlandoEnabled))
            {
                var SelectedLocation = song.SongNum == 0 ? data.GoalSong : data.ApLocationData[song.SongNum];
                var TargetPool = Pools.First(x => x.Name == song.SongPool);
                var TargetSong = song.SongPlandoEnabled ? 
                    song.SongHash : 
                    songPoolManager.GetRandomUnusedSong(TargetPool, UsedSongs, Connection).SongChecksum;
                SelectedLocation.Requirements = TargetPool;
                SelectedLocation.SongHash = TargetSong;
                UsedSongs.SetIfEmpty(TargetPool.Name, []);
                UsedSongs[TargetPool.Name].Add(TargetSong!);
            }

            List<(SongData Data, SongPool Pool)> PickedSongs = [];
            foreach (var p in Pools)
            {
                for (int i = 0; i < p.AmountInPool; i++)
                {
                    var TargetSong = songPoolManager.GetRandomUnusedSong(p, UsedSongs, Connection);
                    PickedSongs.Add((TargetSong, p));
                    UsedSongs.SetIfEmpty(p.Name, []);
                    UsedSongs[p.Name].Add(TargetSong.SongChecksum);
                }
            }
            SongLocation[] SongLocations = [data.GoalSong, .. data.ApLocationData.Values];
            SongLocations = [.. SongLocations.Where(x => !PlandoSongData[x.SongNumber].HasPlando)];

            if (PickedSongs.Count != SongLocations.Length)
                throw new Exception($"Mismatched song pool from Picked Songs {PickedSongs.Count}|{SongLocations.Length}");

            foreach (var i in SongLocations)
            {
                int randomIndex = Connection.GetRNG().Next(PickedSongs.Count);
                var pickedSong = PickedSongs[randomIndex];
                i.SongHash = pickedSong.Data.SongChecksum;
                i.Requirements = pickedSong.Pool;
                PickedSongs.RemoveAt(randomIndex);
            }

            foreach (var song in PlandoSongData.Values.Where(x => !x.PoolPlandoEnabled && x.SongPlandoEnabled))
            {
                var SelectedLocation = song.SongNum == 0 ? data.GoalSong : data.ApLocationData[song.SongNum];
                var ChosenPool = songPoolManager.TryGetRandomUnusedPool(song.SongHash!, UsedSongs, Connection);
                SelectedLocation.SongHash = song.SongHash;
                SelectedLocation.Requirements = ChosenPool;
                UsedSongs.SetIfEmpty(ChosenPool.Name, []);
                UsedSongs[ChosenPool.Name].Add(song.SongHash!);
            }

        }
    }
}
