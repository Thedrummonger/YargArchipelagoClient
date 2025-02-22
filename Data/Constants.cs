using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
