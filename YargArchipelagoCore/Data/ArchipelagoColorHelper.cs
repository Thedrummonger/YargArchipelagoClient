extern alias TDMAP;
using System.Drawing;

namespace YargArchipelagoCore.Data
{
    public static class ArchipelagoColorHelper
    {
        public static Color ConvertToSystemColor(this TDMAP.Archipelago.MultiClient.Net.Models.Color archipelagoColor)
        {
            return Color.FromArgb(archipelagoColor.R, archipelagoColor.G, archipelagoColor.B);
        }

        public class Hints
        {
            public static readonly TDMAP.Archipelago.MultiClient.Net.Models.Color Unfound = TDMAP.Archipelago.MultiClient.Net.Models.Color.Red;
            public static readonly TDMAP.Archipelago.MultiClient.Net.Models.Color found = TDMAP.Archipelago.MultiClient.Net.Models.Color.Green;
        }
        /// <summary>
        /// Predefined color mappings for player-related messages.
        /// </summary>
        public class Players
        {
            //NOTE these are backwards in the comments in the library
            public static readonly TDMAP.Archipelago.MultiClient.Net.Models.Color Local = TDMAP.Archipelago.MultiClient.Net.Models.Color.Magenta;
            public static readonly TDMAP.Archipelago.MultiClient.Net.Models.Color Other = TDMAP.Archipelago.MultiClient.Net.Models.Color.Yellow;
        }
        /// <summary>
        /// Predefined color mappings for item-related messages.
        /// </summary>
        public class Items
        {
            public static readonly TDMAP.Archipelago.MultiClient.Net.Models.Color Normal = TDMAP.Archipelago.MultiClient.Net.Models.Color.Cyan;
            public static readonly TDMAP.Archipelago.MultiClient.Net.Models.Color Important = TDMAP.Archipelago.MultiClient.Net.Models.Color.SlateBlue;
            public static readonly TDMAP.Archipelago.MultiClient.Net.Models.Color Traps = TDMAP.Archipelago.MultiClient.Net.Models.Color.Salmon;
            public static readonly TDMAP.Archipelago.MultiClient.Net.Models.Color Progression = TDMAP.Archipelago.MultiClient.Net.Models.Color.Plum;
        }
        /// <summary>
        /// Predefined color mappings for location-related message types.
        /// </summary>
        public class Locations
        {
            public static readonly TDMAP.Archipelago.MultiClient.Net.Models.Color Entrance = TDMAP.Archipelago.MultiClient.Net.Models.Color.Blue;
            public static readonly TDMAP.Archipelago.MultiClient.Net.Models.Color Location = TDMAP.Archipelago.MultiClient.Net.Models.Color.Green;
        }

    }
}
