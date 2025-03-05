using System.ComponentModel;

namespace YargArchipelagoClient.Data
{
    public static class Constants
    {
        public enum Instrument
        {
            Guitar,
            Bass,
            Drums,
            Keys,
            rhythm,
            vocals
        }
        public enum CompletionReq
        {
            Clear,
            ThreeStar,
            FourStar,
            FiveStar,
            SixStar,
            SevenStar,
            FullCombo
        }
        public enum Difficulty
        {
            Easy,
            Medium,
            Hard,
            Expert
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
            [Description("Restart Trap")]
            TrapRestart
        }
    }
}
