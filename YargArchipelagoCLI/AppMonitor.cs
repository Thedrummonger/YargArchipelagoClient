using YargArchipelagoClient.Data;
using YargArchipelagoClient.Helpers;

namespace YargArchipelagoCLI
{
    public abstract class Applet
    {
        public bool IsEnabled = true;
        public int valueSize = 0;
        public int TotalSize => valueSize + 2; //Size of the values plus header and separator
        public int startIndex = 0;
        public int NeedsUpdate = 0;
        public int currentPage = 0;
        public int maxPage = 0;
        public abstract string Title();
        public abstract bool StaticSize();
        public abstract bool StartAtEnd();
        public abstract string[] Values();
    }

    public class StatusApplet(ConnectionData connection, ConfigData config) : Applet
    {
        public override bool StartAtEnd() => false;
        public override bool StaticSize() => true;
        public override string Title() => "Status";

        public override string[] Values()
        {
            string[] Monitors = [
                $"YARG Connected: {connection.IsConnectedToYarg}",
                $"AP Connection: {connection.SlotName}@{connection.Address}",
                $"Currently Playing: {connection.GetCurrentlyPlaying()?.GetSongDisplayName() ?? "None"}",
                $"Current Fame: {connection.GetCurrentFame()}/{config.FamePointsNeeded}"
            ];
            return Monitors;
        }
    }
    public class ChatApplet(List<string> Messages) : Applet
    {
        public override bool StartAtEnd() => true;
        public override bool StaticSize() => false;
        public override string Title() => "AP Chat";

        public override string[] Values() => [..Messages];
    }
    public class SongApplet(ConnectionData connection, ConfigData config) : Applet
    {
        public override bool StartAtEnd() => false;
        public override bool StaticSize() => false;
        public override string Title() => "Available Songs";

        public override string[] Values() => [..connection.GetAllAvailableSongLocations(config, false).Select(x => $"{x.SongNumber}. {x.GetSongDisplayName(config)} [{x.Requirements?.Name}]")];
    }

    class AppMonitor
    {
        ConfigData _config;
        ConnectionData _connection;
        List<string> ChatLog = [];
        int MenuIndex = 0;
        Applet[] applets;
        Applet SelectedApplet;
        string MenuBar => $"[Esc] Menu [R] Refresh [↕] Cycle Selected App ({SelectedApplet.Title()}) [↔] Cycle App Page [Space] Toggle App";
        public AppMonitor(ConfigData config, ConnectionData connection)
        {
            _config = config;
            _connection = connection;
            applets = [
                new StatusApplet(_connection, _config),
                new SongApplet(_connection, _config),
                new ChatApplet(ChatLog)
            ];
            SelectedApplet = applets.First();
        }

        public void FlagForUpdate<T>(int Delay = 1)
        {
            foreach(var i in applets)
                if (i is T && i.NeedsUpdate == 0)
                    i.NeedsUpdate = Delay;
        }

        public void LogChat(string chat)
        {
            ChatLog.Add(chat);
            if (ChatLog.Count > 500)
                ChatLog.RemoveAt(0);
        }

        public void Show()
        {
            using var cts = new CancellationTokenSource();
            var lastTick = Environment.TickCount64;
            var refreshMs = 500;
            FormatWindow();
            Console.CursorVisible = false;
            while (!cts.IsCancellationRequested)
            {
                Console.SetCursorPosition(0, 0);
                while (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    Console.CursorVisible = false;
                    switch (key)
                    {
                        case ConsoleKey.Escape: return;
                        case ConsoleKey.R: FormatWindow(); break;
                        case ConsoleKey.UpArrow: SelectedApplet = GetNextApplet(applets, SelectedApplet, true); FormatWindow(); break;
                        case ConsoleKey.DownArrow: SelectedApplet = GetNextApplet(applets, SelectedApplet); FormatWindow(); break;
                        case ConsoleKey.Spacebar: SelectedApplet.IsEnabled = !SelectedApplet.IsEnabled; FormatWindow(); break;
                        case ConsoleKey.RightArrow: SelectedApplet.currentPage++; PrintApp(SelectedApplet); break;
                        case ConsoleKey.LeftArrow: SelectedApplet.currentPage--; PrintApp(SelectedApplet); break;
                    }
                }

                if (Environment.TickCount64 - lastTick >= refreshMs)
                {
                    UpdateApps();
                    lastTick = Environment.TickCount64;
                }

                Thread.Sleep(10);
            }
            Console.CursorVisible = true;
        }

        private void UpdateApps()
        {
            foreach (var i in applets.Where(x => x.IsEnabled && x.NeedsUpdate > 0))
            {
                i.NeedsUpdate--;
                if (i.NeedsUpdate == 0)
                    PrintApp(i);
            }
        }

        private void FormatWindow()
        {
            Console.Clear();
            CalculateAppProperties();
            foreach (var i in applets.Where(x => x.IsEnabled))
                PrintApp(i, true);
            Console.SetCursorPosition(0, MenuIndex);
            Console.WriteLine(MenuBar);
        }

        private void PrintApp(Applet app, bool full = false)
        {
            string[][] pages = app.StartAtEnd() ? [.. CLIHelpers.ChunkFromStart(app.Values(), app.valueSize).Reverse()] : [.. app.Values().Chunk(app.valueSize)];
            app.maxPage = Math.Max(pages.Length - 1, 0);
            app.currentPage = Math.Clamp(app.currentPage, 0, app.maxPage);

            var row = app.startIndex;
            var page = pages.Length > 0 ? pages[app.currentPage] : [];

            Console.SetCursorPosition(0, row);
            if (full || pages.Length > 1) 
            {
                string Title = "";
                if (SelectedApplet == app) Title += "> ";
                Title += app.Title();
                if (pages.Length > 1) Title += $" {app.currentPage + 1}/{app.maxPage + 1}";
                Console.Write(Title.PadRight(Console.WindowWidth));
            }
            row++;
            for (int i = 0; i < app.valueSize; i++)
            {
                Console.SetCursorPosition(0, row++);
                Console.Write(((i < page.Length ? page[i] : string.Empty)).PadRight(Console.WindowWidth));
            }
            if (full) 
            { 
                Console.SetCursorPosition(0, row); 
                Console.Write(new string('=', Console.WindowWidth)); 
            }
        }

        public static T GetNextApplet<T>(IList<T> list, T current, bool reverse = false)
        {
            int i = list.IndexOf(current);
            if (i == -1)
                return list[0];

            int next = reverse ? (i - 1 + list.Count) % list.Count  : (i + 1) % list.Count;

            return list[next];
        }

        public static T[] TakePortion<T>(T[] source, int count, bool fromEnd) => fromEnd ? [.. source.TakeLast(count)] : [.. source.Take(count)];

        private void CalculateAppProperties()
        {
            var EnabledApps = applets.Where(x => x.IsEnabled);

            var AvailableConsoleSpace = Console.WindowHeight - 2; //Minus one for buffer and one for menu
            var StaticApps = EnabledApps.Where(x => x.StaticSize());
            var DynamicApps = EnabledApps.Where(x => !x.StaticSize());

            foreach (var app in StaticApps)
                app.valueSize = app.Values().Length;

            int StaticCount = StaticApps.Select(x => x.TotalSize).Sum();
            int DynamicAvailableCount = AvailableConsoleSpace - StaticCount;
            int PerDynamic = DynamicApps.Count() > 0 ? DynamicAvailableCount / DynamicApps.Count() : 0;

            foreach (var app in DynamicApps)
                app.valueSize = PerDynamic - 2; //This value tracks items and does not account for the separator and Title

            int Ind = 0;
            foreach (var app in EnabledApps)
            {
                app.startIndex = Ind;
                Ind = Ind + app.TotalSize;
                app.currentPage = 0;
            }
            MenuIndex = Ind;
        }

    }
}
