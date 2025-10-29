using System.Collections.Generic;

namespace YargArchipelagoCLI
{
    public enum ReturnFlag
    {
        Cancel = 0
    }
    public enum SectionPlacement { Pre, Post }

    public record Option<T>(string Display, T? Tag, Func<bool>? Conditional = null);

    public record FlaggedOption<T>(string Display, ReturnFlag Flag, T? Tag = default, Func<bool>? Conditional = null) : Option<T>(Display, Tag, Conditional);

    public sealed class ConsoleSelect<T>
    {
        readonly List<Option<T>> _options = [];
        readonly List<Option<T>> _staticOptions = [];
        readonly List<string> _pre = [];
        readonly List<string> _post = [];
        int _startIndex;
        int _startPage;
        ConsoleKey? CancelKey = ConsoleKey.Escape;

        public int CurrentPage { get; private set; }
        public int CurrentSelection { get; private set; }

        public ConsoleSelect<T> AddText(SectionPlacement where, params string[] lines)
        {
            if (lines is not { Length: > 0 }) return this;

            foreach (string line in lines)
            {
                List<string> ToAdd = [];
                ToAdd.AddRange(line.Split('\n'));
                if (where == SectionPlacement.Pre) _pre.AddRange(ToAdd);
                else _post.AddRange(ToAdd);

            }
            return this;
        }

        public ConsoleSelect<T> AddSeparator(SectionPlacement where, char ch = '=')
        {
            if (where == SectionPlacement.Pre) _pre.Add(new string(ch, Console.WindowWidth));
            else _post.Add(new string(ch, Console.WindowWidth));
            return this;
        }

        public ConsoleSelect<T> Add(string display, T tag, Func<bool>? when = null) { _options.Add(new(display, tag, when)); return this; }
        public ConsoleSelect<T> Add(Option<T> option) { _options.Add(option); return this; }
        public ConsoleSelect<T> AddRange(IEnumerable<Option<T>> options) { _options.AddRange(options); return this; }
        public ConsoleSelect<T> StartIndex(int i) { _startIndex = i; return this; }
        public ConsoleSelect<T> StartPage(int i) { _startPage = i; return this; }
        public ConsoleSelect<T> AddStatic(string display, T tag, Func<bool>? when = null) { _staticOptions.Add(new(display, tag, when)); return this; }
        public ConsoleSelect<T> AddStatic(Option<T> option) { _staticOptions.Add(option); return this; }
        public ConsoleSelect<T> AddStaticRange(IEnumerable<Option<T>> options) { _staticOptions.AddRange(options); return this; }
        public ConsoleSelect<T> AddCancelOption(string display) { _staticOptions.Add(new FlaggedOption<T>(display, ReturnFlag.Cancel)); return this; }

        public bool HasValidOptions => _options.Any(x => x.Conditional is null || x.Conditional());

        public ConsoleSelect<T> SetCancelKey(ConsoleKey key = ConsoleKey.None) { CancelKey = key; return this; }

        public Option<T> GetSelection()
        {
            Console.Clear();
            Console.CursorVisible = false;

            var ValidOptions = _options.Where(x => x.Conditional is null || x.Conditional()).ToArray();

            var MaxOptions = Math.Max(Console.WindowHeight - _pre.Count - _post.Count - _staticOptions.Count - 1, 1);
            var NeedsPages = MaxOptions < ValidOptions.Length;
            if (NeedsPages)
                MaxOptions = Math.Max(MaxOptions - 1, 1); //Make space for the Page Counter

            var OptionPages = ValidOptions.Chunk(MaxOptions).ToArray();
            for (int i = 0; i < OptionPages.Length; i++)
                OptionPages[i] = [.. OptionPages[i], .. _staticOptions];

            CurrentPage = Math.Clamp(_startPage, 0, OptionPages.Length - 1);
            CurrentSelection = Math.Clamp(_startIndex, 0, OptionPages[CurrentPage].Length - 1);

            //Write Lines. Write Empty Space for drawable.
            foreach (var l in _pre) Console.WriteLine(l);
            if (NeedsPages) { Console.WriteLine(); }
            for (var i = 0; i < OptionPages.Select(x => x.Length).Max(); i++) { Console.WriteLine(); }
            foreach (var l in _post) Console.WriteLine(l);
            while (true)
            {
                Render(OptionPages, CurrentSelection, _pre.Count, CurrentPage);
                var Key = Console.ReadKey(true).Key;
                if (Key == CancelKey)
                {
                    Console.CursorVisible = true;
                    Console.Clear();
                    return new FlaggedOption<T>(string.Empty, ReturnFlag.Cancel);
                }
                switch (Key)
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
                        CurrentSelection = Math.Clamp(CurrentSelection, 0, OptionPages[CurrentPage].Length - 1);
                        break;
                    case ConsoleKey.PageUp:
                    case ConsoleKey.RightArrow:
                        CurrentPage = Math.Clamp(CurrentPage + 1, 0, OptionPages.Length - 1);
                        CurrentSelection = Math.Clamp(CurrentSelection, 0, OptionPages[CurrentPage].Length - 1);
                        break;
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
            int OptionLength = OptionPages.Select(x => x.Length).Max();
            int CurrentInd = OptionStartIndex;
            Console.SetCursorPosition(0, CurrentInd);
            var Options = OptionPages[CurrentPage];
            if (OptionPages.Length > 1)
            {
                Console.Write($"Page {CurrentPage + 1}/{OptionPages.Length}".PadRight(Console.WindowWidth));
                CurrentInd++;
            }
            int Index = 0;
            for(var i = 0; i < OptionLength; i++)
            {
                Console.SetCursorPosition(0, CurrentInd);
                var f = Console.ForegroundColor; 
                var b = Console.BackgroundColor;
                if (Index == CurrentSelection) 
                { 
                    Console.BackgroundColor = ConsoleColor.DarkGray; 
                    Console.ForegroundColor = ConsoleColor.Black; 
                }
                if (i < Options.Length)
                    Console.Write(Options[i].Display.PadRight(Console.WindowWidth));
                else
                    Console.Write(string.Empty.PadRight(Console.WindowWidth));
                Console.ForegroundColor = f; 
                Console.BackgroundColor = b;
                CurrentInd++;
                Index++;
            }
        }
    }
}
