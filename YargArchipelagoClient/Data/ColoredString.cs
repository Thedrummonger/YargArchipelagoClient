using System.Text;

namespace YargArchipelagoClient.Data
{
    public class ColoredString
    {
        private List<(string Text, Color? Color)> words;

        public ColoredString()
        {
            words = [];
        }

        public ColoredString(string text)
        {
            words = [];
            AddText(text);
        }

        public ColoredString(string text, Color defaultColor)
        {
            words = [];
            AddText(text, defaultColor);
        }

        // Adds a word with default color (black), auto-spaces by default
        public ColoredString AddText(string text, bool addSpace = true)
        {
            words.Add((text + (addSpace ? " " : ""), null));
            return this;
        }

        // Adds a word with a specific color, auto-spaces by default
        public ColoredString AddText(string text, Color color, bool addSpace = true)
        {
            words.Add((text + (addSpace ? " " : ""), color));
            return this;
        }

        // Gets the plain text version without trailing spaces
        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (var (text, _) in words)
            {
                sb.Append(text);
            }
            return sb.ToString().TrimEnd(); // Remove final space
        }

        // Exposes the words for rendering
        public List<(string Text, Color? Color)> Words => words;
    }
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

        /// <summary>
        /// Builds an RTF snippet for the given colored strings.
        /// </summary>
        public static string BuildColoredStringsRtf(RichTextBox rtb, IEnumerable<ColoredString> coloredStrings)
        {
            Dictionary<Color, int> colorMap = [];
            int nextIndex = 1;
            foreach (var cs in coloredStrings)
                foreach (var (word, color) in cs.Words)
                {
                    var c = color ?? rtb.ForeColor;
                    if (!colorMap.ContainsKey(c))
                        colorMap[c] = nextIndex++;
                }
            var sb = new StringBuilder(@"{\rtf1\ansi")
                .Append(@"{\colortbl ;")
                .Append(string.Join("", colorMap.Select(kv => $@"\red{kv.Key.R}\green{kv.Key.G}\blue{kv.Key.B};")))
                .Append('}');
            foreach (var cs in coloredStrings)
            {
                foreach (var (word, color) in cs.Words)
                    sb.Append($@"\cf{colorMap[color ?? rtb.ForeColor]} {EscapeRtf(word)}");
                sb.Append(@"\line ");
            }
            return sb.Append('}').ToString();
        }

        /// <summary>
        /// Sanitize the given word so it's not parsed as RTF
        /// </summary>
        public static string EscapeRtf(string text) =>
            text is null ? "" : text.Replace(@"\", @"\\").Replace("{", @"\{").Replace("}", @"\}");

    }
}
