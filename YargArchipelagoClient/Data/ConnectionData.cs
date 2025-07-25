﻿using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using TDMUtils;

namespace YargArchipelagoClient.Data
{
    public class ConnectionData
    {
        public static string GetSeedPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seeds");

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
        private APPacketServer PacketServer;
        public APPacketServer GetPacketServer(ConfigData Config)
        {
            if (PacketServer is not null) throw new Exception("Packet server was already started!");
            PacketServer = new APPacketServer(Config, this);
            return PacketServer;
        }
        public APPacketServer GetPacketServer()
        {
            if (PacketServer is null) throw new Exception("Attempted to retrieve packet server before it was started!");
            return PacketServer;
        }
        
        [JsonIgnore]
        public HashSet<int> ReceivedSongs { get; } = [];
        [JsonIgnore]
        public Dictionary<APWorldData.StaticItems, int> ReceivedStaticItems { get; } = [];
        [JsonIgnore]
        public HashSet<long> CheckedLocations { get; } = [];
        [JsonIgnore]
        public DeathLinkService? DeathLinkService { get; }
        [JsonIgnore]
        public YargArchipelagoCommon.CommonData.SongData? CurrentlyPlaying = null;

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

        public void UpdateReceivedItems()
        {
            ReceivedStaticItems.Clear();
            foreach (var i in Session.Items.AllItemsReceived)
            {
                if (APWorldData.APIDs.StaticItemIDs.TryGetValue(i.ItemId, out var item))
                {
                    ReceivedStaticItems.SetIfEmpty(item, 0);
                    ReceivedStaticItems[item]++;
                    continue;
                }
                if (APWorldData.APIDs.SongItemIds.TryGetValue(i.ItemId, out var songItem))
                {
                    ReceivedSongs.Add(songItem);
                    continue;
                }
                throw new Exception($"Error, received unknown item {i.ItemName} [{i.ItemId}]");
            }
            if (ReceivedStaticItems.TryGetValue(APWorldData.StaticItems.Victory, out var v) && v > 0)
                Session.SetGoalAchieved();
        }

        public string getSaveFileName() =>
            $"{Session.RoomState.Seed}_{SlotName}_{Session.Players.ActivePlayer.Slot}_{Session.Players.ActivePlayer.GetHashCode()}";
    }
}
