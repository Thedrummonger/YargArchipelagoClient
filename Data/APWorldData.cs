using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YargArchipelagoClient.Data
{
    public class APWorldData
    {
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
        public enum StaticItems
        {
            [Description("Victory")]
            Victory,
            [Description("Fame Point")]
            FamePoint,
            [Description("Swap Song (Random)")]
            SwapRandom,
            [Description("Swap Song (Pick)")]
            SwapPick,
            [Description("Lower Difficulty")]
            LowerDifficulty,
            [Description("Restart Trap")]
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

            public static Dictionary<long, StaticItems> Items { get; } =
                Enum.GetValues(typeof(StaticItems))
                    .Cast<StaticItems>()
                    .Select((item, index) => new { Key = rootID + index, Value = item })
                    .ToDictionary(x => x.Key, x => x.Value);

            public static Dictionary<long, int> SongItemIds =>
                Enumerable.Range(1, MaxSongs)
                          .ToDictionary(x => Items.Keys.Max() + x, x => x);

            public static Dictionary<long, StaticLocations> Locations { get; } =
                Enum.GetValues(typeof(StaticLocations))
                    .Cast<StaticLocations>()
                    .Select((item, index) => new { Key = rootID + index, Value = item })
                    .ToDictionary(x => x.Key, x => x.Value);

            public static Dictionary<long, (int songnum, LocationType locType)> SongLocationIDs =>
                Enumerable.Range(1, MaxSongs).SelectMany(songnum => new[]
                {
                    (Key: Locations.Keys.Max() + (songnum - 1) * 3 + 1, Value: (songnum, LocationType.standard)),
                    (Key: Locations.Keys.Max() + (songnum - 1) * 3 + 2, Value: (songnum, LocationType.extra)),
                    (Key: Locations.Keys.Max() + (songnum - 1) * 3 + 3, Value: (songnum, LocationType.fame))
                }).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
