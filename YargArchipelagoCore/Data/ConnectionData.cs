extern alias TDMAP;
using TDMAP.Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using TDMAP.Archipelago.MultiClient.Net;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using TDMUtils;
using YargArchipelagoCore.Helpers;
using static YargArchipelagoCore.Data.APWorldData;

namespace YargArchipelagoCore.Data
{
    public class ConnectionData
    {

        public ConnectionData(string? address, string slotname, string password, ArchipelagoSession session)
        {
            Session = session;
            Address = address;
            SlotName = slotname;
            Password = password;
            if (session is not null)
                DeathLinkService = session?.CreateDeathLinkService();
        }
        public string? Address { get; private set; }
        public string? SlotName { get; private set; }
        public string? Password { get; private set; }
        public ArchipelagoSession GetSession() => Session;
        private readonly ArchipelagoSession Session;

        private Random? SeededRNG = null;

        [JsonIgnore]
        private APPipeServer PacketServer;
        [JsonIgnore]
        public YargClientSyncHelper clientSyncHelper;
        [JsonIgnore]
        public CoreEventManager eventManager;
        public APPipeServer CreatePacketServer(ConfigData Config)
        {
            if (PacketServer is not null) throw new Exception("Packet server was already started!");
            PacketServer = new APPipeServer(Config, this);
            return PacketServer;
        }
        public APPipeServer GetPacketServer()
        {
            if (PacketServer is null) throw new Exception("Attempted to retrieve packet server before it was started!");
            return PacketServer;
        }

        [JsonIgnore]
        public Dictionary<int, BaseYargAPItem> ReceivedSongs { get; } = [];
        [JsonIgnore]
        public HashSet<long> CheckedLocations { get; } = [];
        [JsonIgnore]
        public HashSet<StaticYargAPItem> ApItemsRecieved { get; } = [];
        [JsonIgnore]
        public DeathLinkService? DeathLinkService { get; }
        [JsonIgnore]
        private YargArchipelagoCommon.CommonData.SongData? CurrentlyPlaying = null;
        [JsonIgnore]
        public bool IsConnectedToYarg => PacketServer is not null && PacketServer.IsConnected;

        public void SetCurrentlyPlaying(YargArchipelagoCommon.CommonData.SongData? song = null) => CurrentlyPlaying = song;
        public YargArchipelagoCommon.CommonData.SongData? GetCurrentlyPlaying() => CurrentlyPlaying;
        public bool IsCurrentlyPlayingSong(out YargArchipelagoCommon.CommonData.SongData? CurrentSong)
        {
            CurrentSong = CurrentlyPlaying;
            return CurrentlyPlaying is not null;
        }
        public void UpdateCheckedLocations()
        {
            foreach (var i in Session.Locations.AllLocationsChecked)
                CheckedLocations.Add(i);
        }

        public Random GetRNG()
        {
            SeededRNG ??= new(GetAPSeed());
            return SeededRNG;
        }

        private int GetAPSeed()
        {
            byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(Session!.RoomState.Seed));
            return BitConverter.ToInt32(hash, 0);
        }

        public void UpdateDeathLinkTags(ConfigData config)
        {
            if (Session is null || !Session.Socket.Connected || DeathLinkService is null)
                return;
            if (config.DeathLinkMode > YargArchipelagoCommon.CommonData.DeathLinkType.None)
                DeathLinkService.EnableDeathLink();
            else
                DeathLinkService.DisableDeathLink();
        }

        public void UpdateReceivedItems(ConfigData configData)
        {
            Dictionary<StaticItems, int> ServerLocProxy = [];
            foreach (var i in Session.Items.AllItemsReceived)
            {
                if (APIDs.StaticItemIDs.TryGetValue(i.ItemId, out var item))
                {
                    if (i.Player.Slot == 0)
                    {
                        ServerLocProxy.SetIfEmpty(item, 0);
                        ServerLocProxy[item]++;
                    }
                    ApItemsRecieved.Add(new StaticYargAPItem(item, i.ItemId, i.Player.Slot, i.Player.Slot == 0 ? ServerLocProxy[item] : i.LocationId, i.LocationGame));
                    if (item == StaticItems.Victory)
                        Session.SetGoalAchieved();
                    continue;
                }
                if (APIDs.SongItemIds.TryGetValue(i.ItemId, out var songItem))
                {
                    ReceivedSongs[songItem] = new(i.ItemId, i.Player.Slot, i.LocationId, i.LocationGame);
                    continue;
                }
                throw new Exception($"Error, received unknown item {i.ItemName} [{i.ItemId}]");
            }
        }
        public string getSaveFileName() =>
            $"{Session.RoomState.Seed}_{SlotName}_{Session.Players.ActivePlayer.Slot}_{Session.Players.ActivePlayer.GetHashCode()}";
    }
}
