extern alias TDMAP;
using TDMAP.Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using TDMAP.Archipelago.MultiClient.Net.MessageLog.Messages;
using Newtonsoft.Json;
using System.Diagnostics;
using TDMUtils;
using YargArchipelagoCore.Data;
using YargArchipelagoCommon;

namespace YargArchipelagoCore.Helpers
{
    public static class ClientInitializationHelper
    {
        public static bool ConnectToServer(out ConnectionData? connection, Func<ConnectionData?> CreateNewConnection)
        {
            connection = CreateNewConnection();
            return connection is not null &&
                connection.GetSession() is not null &&
                connection.GetSession()!.Socket.Connected;
        }

        public static bool GetConfig(ConnectionData Connection, out ConfigData? configData, Func<ConfigData?> CreateNewConfig)
        {
            configData = null;
            Directory.CreateDirectory(CommonData.SeedConfigPath);

            var ConfigFile = Directory.GetFiles(CommonData.SeedConfigPath).FirstOrDefault(file => Path.GetFileName(file) == Connection.getSaveFileName());
            if (ConfigFile is not null)
            {
                Debug.WriteLine($"Seed Found {ConfigFile}");
                try { configData = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(ConfigFile)); }
                catch { configData = null; }
            }
            if (configData is null)
            {
                configData = CreateNewConfig();
                if (configData is not null)
                    ReadSlotData(Connection, configData);
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

            if (SlotData.TryGetValue("death_link", out var DLO) && DLO is Int64 DLI)
            {
                config.DeathLinkMode = (CommonData.DeathLinkType)DLI;
                config.YamlDeathLink = (CommonData.DeathLinkType)DLI;
            }
            if (SlotData.TryGetValue("energy_link", out var ELO) && ELO is Int64 ELI)
            {
                config.EnergyLinkMode = (CommonData.EnergyLinkType)ELI;
                config.YamlEnergyLink = (CommonData.EnergyLinkType)ELI;
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

        public static bool ConnectSession(Func<ConnectionData?> CreateNewConnection, Func<ConfigData?> CreateNewConfig, Action<ConnectionData> ApplyUIListeners, out ConnectionData? Connection, out ConfigData? Config)
        {
            Directory.CreateDirectory(CommonData.DataFolder);
            Connection = null;
            Config = null;

            var UserConfig = DataFileUtilities.LoadObjectFromFileOrDefault(CommonData.userConfigFile, new CommonData.UserConfig(), true);

            if (!ConnectToServer(out var connectResult, CreateNewConnection))
                return false;
            Connection = connectResult!;
            File.WriteAllText(CommonData.ConnectionCachePath, Connection.ToFormattedJson());

            if (!GetConfig(Connection, out var configResult, CreateNewConfig))
                return false;
            Config = configResult!;
            Config.CurrentUserConfig = UserConfig;
            Config.SaveConfigFile(Connection);

            Debug.WriteLine($"The Following Songs were not valid for any profile in this config\n\n{Config.GetUnusableSongs().Select(x => x.GetSongDisplayName()).ToFormattedJson()}");

            Connection.UpdateDeathLinkTags(Config);

            var PacketServer = Connection!.CreatePacketServer(Config);
            _ = PacketServer.StartAsync();

            Connection.clientSyncHelper = new YargClientSyncHelper(Connection, Config);

            Connection.eventManager = new CoreEventManager(Config, Connection);
            Connection.eventManager.ApplyEvents();
            ApplyUIListeners(Connection);

            Connection.clientSyncHelper.SyncTimerTick(null,null);
            Connection.clientSyncHelper.StartTimer();

            return Config is not null && Connection is not null;
        }
        public static async Task DisconnectSession(this ConnectionData Connection, Action<ConnectionData> RemoveUIListeners)
        {
            Connection.clientSyncHelper?.StopTimer();
            Connection.eventManager?.RemoveEvents();
            RemoveUIListeners(Connection);
            try { Connection.GetPacketServer()?.Stop(); } catch { }
            try { await Connection.GetSession().Socket.DisconnectAsync(); } catch { }
        }
    }

    public class CoreEventManager(ConfigData config, ConnectionData connection)
    {
        public void ApplyEvents()
        {
            connection.GetSession().MessageLog.OnMessageReceived += RelayChatToYARG;
            connection.GetSession().Items.ItemReceived += Items_ItemReceived;
            connection.GetSession().Locations.CheckedLocationsUpdated += Locations_CheckedLocationsUpdated;
            connection.DeathLinkService!.OnDeathLinkReceived += DeathLinkService_OnDeathLinkReceived;
        }

        public void RemoveEvents()
        {
            connection.GetSession().MessageLog.OnMessageReceived -= RelayChatToYARG;
            connection.GetSession().Items.ItemReceived -= Items_ItemReceived;
            connection.GetSession().Locations.CheckedLocationsUpdated -= Locations_CheckedLocationsUpdated;
            connection.DeathLinkService!.OnDeathLinkReceived -= DeathLinkService_OnDeathLinkReceived;
        }

        private void DeathLinkService_OnDeathLinkReceived(DeathLink deathLink) 
        {
            if (config.DeathLinkMode <= CommonData.DeathLinkType.None) return;
            _ = connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket
            {
                deathLinkData = new CommonData.DeathLinkData(deathLink.Source, deathLink.Cause, config.DeathLinkMode)
            });
        }
        private void Locations_CheckedLocationsUpdated(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations) => connection.clientSyncHelper.ShouldUpdate = true;
        private void Items_ItemReceived(TDMAP.Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper helper) => connection.clientSyncHelper.ShouldUpdate = true;
        private void RelayChatToYARG(LogMessage message)
        {
            if (message is ItemSendLogMessage ItemLog && ShouldRelay(ItemLog))
                _ = connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket { Message = message.ToString() });
            else if (message is ChatLogMessage && config.InGameAPChat)
                _ = connection.GetPacketServer().SendPacketAsync(new CommonData.Networking.YargAPPacket { Message = message.ToString() });

            bool ShouldRelay(ItemSendLogMessage IL)
            {
                if (config.InGameItemLog == CommonData.ItemLog.ToMe) 
                    return IL.IsReceiverTheActivePlayer || IL.IsSenderTheActivePlayer;
                return config.InGameItemLog == CommonData.ItemLog.All;
            }
        }
    }
}
