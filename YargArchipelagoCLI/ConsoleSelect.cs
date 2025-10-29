using static System.Net.Mime.MediaTypeNames;

namespace YargArchipelagoCLI
{
    public enum SectionPlacement { Pre, Post }

    public sealed record Option<T>(string Display, T Tag, Func<bool>? Conditional = null);

    public sealed class ConsoleSelect<T>
    {
        readonly List<Option<T>> _options = [];
        readonly List<string> _pre = [];
        readonly List<string> _post = [];
        int _startIndex;
        int _startPage;
        bool _includeCancel => !string.IsNullOrWhiteSpace(_cancelLabel);
        string? _cancelLabel = "Cancel";

        public ConsoleSelect<T> AddText(SectionPlacement where, params string[] lines)
        {
            if (lines is not { Length: > 0 }) return this;
            if (where == SectionPlacement.Pre) _pre.AddRange(lines);
            else _post.AddRange(lines);
            return this;
        }

        public ConsoleSelect<T> AddSeparator(SectionPlacement where, char ch = '=')
        {
            if (where == SectionPlacement.Pre) _pre.Add(new string(ch, Console.WindowWidth));
            else _post.Add(new string(ch, Console.WindowWidth));
            return this;
        }

        public ConsoleSelect<T> Add(string display, T tag, Func<bool>? when = null) { _options.Add(new(display, tag, when)); return this; }

        public ConsoleSelect<T> AddRange(IEnumerable<Option<T>> options) { _options.AddRange(options); return this; }

        public ConsoleSelect<T> StartIndex(int i) { _startIndex = i; return this; }
        public ConsoleSelect<T> StartPage(int i) { _startPage = i; return this; }

        public ConsoleSelect<T> IncludeCancel(string? label = null) { _cancelLabel = label; return this; }

        public Option<T>? GetSelection()
        {
            Console.Clear();
            Console.CursorVisible = false;

            var MaxOptions = Math.Max(Console.WindowHeight - _pre.Count - _post.Count - 1, 1);
            var NeedsPages = MaxOptions < _options.Count;
            if (NeedsPages)
                MaxOptions = Math.Max(MaxOptions - 1, 1); //Make space for the Page Counter

            var OptionPages = _options.Chunk(MaxOptions).ToArray();

            int CurrentPage = Math.Clamp(_startPage, 0, OptionPages.Length - 1);
            int CurrentSelection = Math.Clamp(_startIndex, 0, OptionPages[CurrentPage].Length - 1);

            //Write Lines. Write Empty Space for drawable.
            foreach (var l in _pre) Console.WriteLine(l);
            if (NeedsPages) { Console.WriteLine(); }
            foreach (var _ in OptionPages[CurrentPage]) { Console.WriteLine(); }
            foreach (var l in _post) Console.WriteLine(l);
            while (true)
            {
                Render(OptionPages, CurrentSelection, _pre.Count, CurrentPage);
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow:
                        CurrentSelection = Math.Clamp(CurrentSelection - 1, 0, OptionPages[CurrentPage].Length - 1);
                        break;
                    case ConsoleKey.DownArrow:
                        CurrentSelection = Math.Clamp(CurrentSelection + 1, 0, OptionPages[CurrentPage].Length - 1);
                        break;
                    case ConsoleKey.PageDown:
                    case ConsoleKey.LeftArrow:
                        CurrentPage = Math.Clamp(CurrentPage - 1, 0, OptionPages.Length - 1);
                        break;
                    case ConsoleKey.PageUp:
                    case ConsoleKey.RightArrow:
                        CurrentPage = Math.Clamp(CurrentPage + 1, 0, OptionPages.Length - 1);
                        break;
                    case ConsoleKey.Escape when _includeCancel:
                        Console.CursorVisible = true;
                        Console.Clear();
                        return null;
                    case ConsoleKey.Enter:
                    case ConsoleKey.Spacebar:
                        Console.CursorVisible = true;
                        Console.Clear();
                        return OptionPages[CurrentPage][CurrentSelection];
                }
            }

        }

        private static void Render(Option<T>[][] OptionPages, int CurrentSelection, int OptionStartIndex, int CurrentPage)
        {
            int CurrentInd = OptionStartIndex;
            Console.SetCursorPosition(0, CurrentInd);
            var Options = OptionPages[CurrentPage];
            if (OptionPages.Length > 1)
            {
                Console.Write($"Page {CurrentPage + 1}/{OptionPages.Length}".PadRight(Console.WindowWidth));
                CurrentInd++;
            }
            int Index = 0;
            foreach(var i in Options)
            {
                Console.SetCursorPosition(0, CurrentInd);
                var f = Console.ForegroundColor; 
                var b = Console.BackgroundColor;
                if (Index == CurrentSelection) 
                { 
                    Console.BackgroundColor = ConsoleColor.DarkGray; 
                    Console.ForegroundColor = ConsoleColor.Black; 
                }
                Console.Write(i.Display.PadRight(Console.WindowWidth));
                Console.ForegroundColor = f; 
                Console.BackgroundColor = b;
                CurrentInd++;
                Index++;
            }
        }
    }
}
