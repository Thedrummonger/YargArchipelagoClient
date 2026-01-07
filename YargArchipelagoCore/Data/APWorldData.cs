using Archipelago.MultiClient.Net.Models;
using System.ComponentModel;
using YargArchipelagoCommon;
using static YargArchipelagoCore.Data.ArchipelagoColorHelper;

namespace YargArchipelagoCore.Data
{
    public class APWorldData
    {
        public static readonly Version APVersion = new(0, 6, 2);
        public enum CompletionReq
        {
            Clear,
            OneStar,
            TwoStar,
            ThreeStar,
            FourStar,
            FiveStar,
            GoldStar,
            FullCombo
        }

        public class BaseYargAPItem(long itemID, int sendingSlot, long sendingLocationID, string Game)
        {
            public long ItemID = itemID;
            public int SendingPlayerSlot = sendingSlot;
            public long SendingPlayerLocation = sendingLocationID;
            public string SendingPlayerGame = Game;
        }

        public class StaticYargAPItem(StaticItems type, long itemID, int sendingSlot, long sendingLocationID, string Game) : 
            BaseYargAPItem(itemID, sendingSlot, sendingLocationID, Game)
        {
            public StaticItems Type = type;

            private string FillerHash() => $"{Type}|{ItemID}|{SendingPlayerSlot}|{SendingPlayerLocation}|{SendingPlayerGame}";
            public override int GetHashCode() => FillerHash().GetHashCode();

            public override bool Equals(object? obj)
            {
                if (obj == null || GetType() != obj.GetType()) return false;
                StaticYargAPItem other = (StaticYargAPItem)obj;
                return FillerHash() == other.FillerHash();
            }

            public static bool operator ==(StaticYargAPItem left, StaticYargAPItem right)
            {
                if (ReferenceEquals(left, right)) return true;
                if (left is null || right is null) return false;
                return left.Equals(right);
            }

            public static bool operator !=(StaticYargAPItem left, StaticYargAPItem right) => !(left == right);
        }

        [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
        public sealed class FillerTrapTypeAttribute(CommonData.APActionItem type) : Attribute
        {
            public CommonData.APActionItem Type { get; } = type;
        }
        public enum StaticItems
        {
            [Description("Victory")]
            Victory,
            [Description("Fame Point")]
            FamePoint,
            [Description("Star Power"), FillerTrapType(CommonData.APActionItem.StarPower)]
            StarPower,
            [Description("Swap Song (Random)")]
            SwapRandom,
            [Description("Swap Song (Pick)")]
            SwapPick,
            [Description("Lower Difficulty")]
            LowerDifficulty,
            [Description("Restart Trap"), FillerTrapType(CommonData.APActionItem.Restart)]
            TrapRestart
        }
        public enum StaticLocations
        {
            [Description("Goal Song")]
            Goal
        }

        public enum LocationType
        {
            standard = 1,
            extra = 2,
            fame = 3
        }

        public static class APIDs
        {
            public const int MaxSongs = 500;
            public const long rootID = 5874530000;

            public static Dictionary<long, StaticItems> StaticItemIDs { get; } =
                Enum.GetValues(typeof(StaticItems))
                    .Cast<StaticItems>()
                    .Select((item, index) => new { Key = rootID + index, Value = item })
                    .ToDictionary(x => x.Key, x => x.Value);

            public static Dictionary<long, int> SongItemIds =>
                Enumerable.Range(1, MaxSongs)
                          .ToDictionary(x => StaticItemIDs.Keys.Max() + x, x => x);

            public static Dictionary<long, StaticLocations> StaticLocationIDs { get; } =
                Enum.GetValues(typeof(StaticLocations))
                    .Cast<StaticLocations>()
                    .Select((item, index) => new { Key = rootID + index, Value = item })
                    .ToDictionary(x => x.Key, x => x.Value);

            public static Dictionary<long, (int songnum, LocationType locType)> SongLocationIDs =>
                Enumerable.Range(1, MaxSongs).SelectMany(songnum => new[]
                {
                    (Key: StaticLocationIDs.Keys.Max() + (songnum - 1) * 3 + 1, Value: (songnum, LocationType.standard)),
                    (Key: StaticLocationIDs.Keys.Max() + (songnum - 1) * 3 + 2, Value: (songnum, LocationType.extra)),
                    (Key: StaticLocationIDs.Keys.Max() + (songnum - 1) * 3 + 3, Value: (songnum, LocationType.fame))
                }).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
