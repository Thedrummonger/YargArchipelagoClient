using System.Drawing;
using System.Text;
using TDMUtils;

namespace YargArchipelagoCore.Data
{
    public static class ArchipelagoColorHelper
    {
        public static Color ConvertToSystemColor(this Archipelago.MultiClient.Net.Models.Color archipelagoColor)
        {
            return Color.FromArgb(archipelagoColor.R, archipelagoColor.G, archipelagoColor.B);
        }

        public class Hints
        {
            public static readonly Archipelago.MultiClient.Net.Models.Color Unfound = Archipelago.MultiClient.Net.Models.Color.Red;
            public static readonly Archipelago.MultiClient.Net.Models.Color found = Archipelago.MultiClient.Net.Models.Color.Green;
        }
        /// <summary>
        /// Predefined color mappings for player-related messages.
        /// </summary>
        public class Players
        {
            //NOTE these are backwards in the comments in the library
            public static readonly Archipelago.MultiClient.Net.Models.Color Local = Archipelago.MultiClient.Net.Models.Color.Magenta;
            public static readonly Archipelago.MultiClient.Net.Models.Color Other = Archipelago.MultiClient.Net.Models.Color.Yellow;
        }
        /// <summary>
        /// Predefined color mappings for item-related messages.
        /// </summary>
        public class Items
        {
            public static readonly Archipelago.MultiClient.Net.Models.Color Normal = Archipelago.MultiClient.Net.Models.Color.Cyan;
            public static readonly Archipelago.MultiClient.Net.Models.Color Important = Archipelago.MultiClient.Net.Models.Color.SlateBlue;
            public static readonly Archipelago.MultiClient.Net.Models.Color Traps = Archipelago.MultiClient.Net.Models.Color.Salmon;
            public static readonly Archipelago.MultiClient.Net.Models.Color Progression = Archipelago.MultiClient.Net.Models.Color.Plum;
        }
        /// <summary>
        /// Predefined color mappings for location-related message types.
        /// </summary>
        public class Locations
        {
            public static readonly Archipelago.MultiClient.Net.Models.Color Entrance = Archipelago.MultiClient.Net.Models.Color.Blue;
            public static readonly Archipelago.MultiClient.Net.Models.Color Location = Archipelago.MultiClient.Net.Models.Color.Green;
        }

    }
}
