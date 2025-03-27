using Newtonsoft.Json;
using System.Diagnostics;
using TDMUtils;
using YargArchipelagoClient.Data;
using static YargArchipelagoClient.Forms.PlandoForm;

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

        public static bool AssignSongs(ConfigData data, ConnectionData Connection, List<SongPool> Pools, Dictionary<int, PlandoData> PlandoSongData, SongPoolManager songPoolManager)
        {

            Random RNG = Connection.GetRNG();


            SongLocation[] SongLocations = [data.GoalSong, .. data.ApLocationData.Values];
            SongLocation[] NonPlandoSongLocations = [.. SongLocations.Where(x => !PlandoSongData[x.SongNumber].HasValidPlando)];

            List<SongPool> selectedSongPools = [];
            // Add each manually configured pool to the list "p.AmountInPool" times
            foreach (var p in Pools.Where(x => !x.RandomAmount))
                for (int i = 0; i < p.AmountInPool; i++)
                    selectedSongPools.Add(p);

            //If there are not enough manually configured pools in the list, add random pools until we hit the needed amount
            var RandomPools = Pools.Where(x => x.RandomAmount);
            var RandomPoolSelections = RandomPools.ToWeightedList(RandomPools.Select(p => p.RandomWeight), RandomPools.Select(songPoolManager.GetPotentialSongsForRandomPool));
            while (selectedSongPools.Count < NonPlandoSongLocations.Length)
                selectedSongPools.Add(RandomPoolSelections.PickRandomWeighted(RNG));

            foreach (var i in Pools.DistinctBy(x => x.Name))
                Debug.WriteLine($"{i.Name} amount: {selectedSongPools.Where(x => x.Name == i.Name).Count()}");

            selectedSongPools.Shuffle(RNG);

            Dictionary<string, HashSet<string>> UsedSongs = [];
            //Assign Song Pool Plandos (including Pool + Song)
            foreach (var song in PlandoSongData.Values.Where(x => x.HasValidPoolPlando))
            {
                var SelectedLocation = data.GetSongLocation(song.SongNum);
                var TargetPool = Pools.First(x => x.Name == song.SongPool);
                var TargetSong = song.HasValidSongPlando ? 
                    song.SongHash : 
                    songPoolManager.GetRandomUnusedSong(TargetPool, UsedSongs, Connection).SongChecksum;
                SelectedLocation.Requirements = TargetPool;
                SelectedLocation.SongHash = TargetSong;
                UsedSongs.SetIfEmpty(TargetPool.Name, []);
                UsedSongs[TargetPool.Name].Add(TargetSong!);
            }

            //Assign songs to standard configured Pool
            var pairedItems = selectedSongPools.Zip(NonPlandoSongLocations, (pool, location) => new { pool, location });
            foreach (var p in pairedItems)
            {
                var TargetSong = songPoolManager.GetRandomUnusedSong(p.pool, UsedSongs, Connection);
                p.location.Requirements = p.pool;
                p.location.SongHash = TargetSong.SongChecksum;
                UsedSongs.SetIfEmpty(p.pool.Name, []);
                UsedSongs[p.pool.Name].Add(TargetSong.SongChecksum);
            }

            //Assign Song only Plandos
            foreach (var song in PlandoSongData.Values.Where(x => !x.HasValidPoolPlando && x.HasValidSongPlando))
            {
                var SelectedLocation = data.GetSongLocation(song.SongNum);
                var ChosenPool = songPoolManager.TryGetRandomUnusedPool(song.SongHash!, UsedSongs, Connection);
                SelectedLocation.SongHash = song.SongHash;
                SelectedLocation.Requirements = ChosenPool;
                UsedSongs.SetIfEmpty(ChosenPool.Name, []);
                UsedSongs[ChosenPool.Name].Add(song.SongHash!);
            }

            foreach (var song in SongLocations)
                if (song.SongHash is null || !data.SongData.ContainsKey(song.SongHash) || song.Requirements is null)
                    throw new Exception($"Not all song locations received song data\n\n{song.ToFormattedJson()}");

            return true;

        }
    }
}
