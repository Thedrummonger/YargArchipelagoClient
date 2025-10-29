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

        public ConsoleSelect<T> IncludeCancel(string? label = null) { _cancelLabel = label; return this; }
        public Option<T>? GetSelection()
        {
            Console.CursorVisible = false;
            foreach (var l in _pre) Console.WriteLine(l);

            var ValidOptions = _options.Where(x => x.Conditional?.Invoke() ?? true).ToArray();
            int ValidOptionsCount = Math.Max(ValidOptions.Length + (_includeCancel ? 1 : 0), 1);

            var (ItemsPerPage, NeedsPages) = Layout(ValidOptionsCount);
            var top = Console.CursorTop + (NeedsPages ? 1 : 0);
            if (NeedsPages) Console.WriteLine();
            for (int i = 0; i < ItemsPerPage; i++) Console.WriteLine();
            foreach (var l in _post) Console.WriteLine(l);

            int CurrentSelection = Math.Clamp(_startIndex, 0, ValidOptionsCount - 1);
            if (ValidOptions.Length == 0 && _includeCancel) CurrentSelection = 0;

            Render();
            while (true)
            {
                var k = Console.ReadKey(true).Key;
                if (k is ConsoleKey.UpArrow or ConsoleKey.DownArrow) 
                { 
                    CurrentSelection = (CurrentSelection + (k == ConsoleKey.UpArrow ? -1 : 1) + ValidOptionsCount) % ValidOptionsCount; 
                    Render(); 
                }
                else if (k is ConsoleKey.Escape && _includeCancel) 
                { 
                    Console.CursorVisible = true; 
                    Console.SetCursorPosition(0, top + ItemsPerPage + _post.Count); 
                    return null; 
                }
                else if (k is ConsoleKey.Enter or ConsoleKey.Spacebar)
                {
                    Console.CursorVisible = true; 
                    Console.SetCursorPosition(0, top + ItemsPerPage + _post.Count);
                    if (ValidOptions.Length == 0 && !_includeCancel) return null;
                    if (_includeCancel && CurrentSelection == ValidOptions.Length) return null;
                    return ValidOptions.Length > CurrentSelection ? ValidOptions[CurrentSelection] : null;
                }
                else if (k is ConsoleKey.PageDown or ConsoleKey.PageUp or ConsoleKey.Home or ConsoleKey.End or ConsoleKey.LeftArrow or ConsoleKey.RightArrow)
                {
                    (ItemsPerPage, _) = Layout(ValidOptionsCount);
                    var page = CurrentSelection / ItemsPerPage; var pages = (ValidOptionsCount + ItemsPerPage - 1) / ItemsPerPage;
                    if (k is ConsoleKey.PageDown or ConsoleKey.RightArrow) page = Math.Min(page + 1, pages - 1);
                    else if (k is ConsoleKey.PageUp or ConsoleKey.LeftArrow) page = Math.Max(page - 1, 0);
                    else if (k is ConsoleKey.Home) page = 0; else page = pages - 1;
                    CurrentSelection = Math.Min(page * ItemsPerPage, ValidOptionsCount - 1); 
                    Render();
                }
            }

            void Render()
            {
                (ItemsPerPage, NeedsPages) = Layout(ValidOptionsCount);
                var CurrentPage = CurrentSelection / ItemsPerPage;
                var TotalPages = (ValidOptionsCount + ItemsPerPage - 1) / ItemsPerPage;
                var FirstItemThisPage = CurrentPage * ItemsPerPage;
                if (NeedsPages) { Console.SetCursorPosition(0, top - 1); var w = Console.WindowWidth; Console.Write(("Page " + (CurrentPage + 1) + "/" + TotalPages).PadRight(w)); }
                for (int i = 0; i < ItemsPerPage; i++)
                {
                    int RederIndex = FirstItemThisPage + i;
                    bool IsRenderingSelected = RederIndex == CurrentSelection;
                    string RenderText = RederIndex < ValidOptions.Length ? "> " + ValidOptions[RederIndex].Display :
                               _includeCancel && RederIndex == ValidOptions.Length ? "> " + _cancelLabel :
                               ValidOptions.Length == 0 && !_includeCancel && i == 0 ? "> (no options)" : "";

                    Console.SetCursorPosition(0, top + i);
                    var f = Console.ForegroundColor; var b = Console.BackgroundColor;
                    if (IsRenderingSelected) { Console.BackgroundColor = ConsoleColor.DarkGray; Console.ForegroundColor = ConsoleColor.Black; }
                    var w = Console.WindowWidth; Console.Write(RenderText.Length > w ? RenderText[..w] : RenderText.PadRight(w));
                    Console.ForegroundColor = f; Console.BackgroundColor = b;
                }
            }

            (int p, bool g) Layout(int n)
            {
                var avail = Console.WindowHeight - _pre.Count - _post.Count - 1;
                if (n > avail) return (Math.Max(1, avail - 1), true);
                return (Math.Max(1, avail), false);
            }
        }



    }
}
