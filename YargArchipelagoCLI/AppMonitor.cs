using TDMUtils.CLITools;
using YargArchipelagoCore.Data;
using YargArchipelagoCore.Helpers;

namespace YargArchipelagoCLI
{

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
    public class ChatApplet() : Applet
    {
        private readonly List<string> ChatLog = [];
        public override bool StartAtEnd() => true;
        public override bool StaticSize() => false;
        public override string Title() => "AP Chat";

        public override string[] Values() => [.. ChatLog];

        public void LogChat(string chat)
        {
            ChatLog.Add(chat);
            if (ChatLog.Count > 500)
                ChatLog.RemoveAt(0);
        }
    }
    public class SongApplet(ConnectionData connection, ConfigData config) : Applet
    {
        public override bool StartAtEnd() => false;
        public override bool StaticSize() => false;
        public override string Title() => "Available Songs";

        public override string[] Values() => [..connection.GetAllAvailableSongLocations(config, false).Select(x => $"{x.SongNumber}. {x.GetSongDisplayName(config)} [{x.Requirements?.Name}]")];
    }
}
