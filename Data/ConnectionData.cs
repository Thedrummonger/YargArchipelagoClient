using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using TDMUtils;
using YargArchipelagoClient.Data;

namespace ArchipelagoPowerTools.Data
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
        public HashSet<int> ReceivedSongs { get; } = [];
        [JsonIgnore]
        public Dictionary<CommonData.StaticItems, int> ReceivedFiller { get; } = [];
        [JsonIgnore]
        public HashSet<long> CheckedLocations { get; } = [];
        [JsonIgnore]
        public DeathLinkService? DeathLinkService { get; }

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
            ReceivedFiller.Clear();
            foreach (var i in Session.Items.AllItemsReceived.Where(x => x.Player == Session.Players.ActivePlayer))
            {
                if (CommonData.APIDs.Items.TryGetValue(i.ItemId, out var item))
                {
                    ReceivedFiller.SetIfEmpty(item, 0);
                    ReceivedFiller[item]++;
                    continue;
                }
                if (CommonData.APIDs.SongItemIds.TryGetValue(i.ItemId, out var songItem))
                {
                    ReceivedSongs.Add(songItem);
                    continue;
                }
                throw new Exception($"Error, received unknown item {i.ItemName} [{i.ItemId}]");
            }
            if (ReceivedFiller.TryGetValue(CommonData.StaticItems.Victory, out var v) && v > 0)
                Session.SetGoalAchieved();
        }

        public string getSaveFileName()
        {
            var c = this!;
            var s = c.GetSession();
            return $"{s.RoomState.Seed}_{c.SlotName}_{s.Players.ActivePlayer.Slot}_{s.Players.ActivePlayer.GetHashCode()}";
        }
    }
}
