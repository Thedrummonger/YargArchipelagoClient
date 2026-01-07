using YargArchipelagoCore.Helpers;
using YargArchipelagoCommon;
using static YargArchipelagoCommon.CommonData;

namespace YargArchipelagoCore.Data
{
    public class SongLocation(int num)
    {
        public int SongNumber = num;
        public string? SongHash = null;
        public long? APStandardCheckLocation = null;
        public long? APExtraCheckLocation = null;
        public long? APFameCheckLocation = null;
        public SongPool? Requirements = null;
        public string GetSongDisplayName(ConfigData config, bool WithArtist = true, bool WithAlbum = false, bool WithSongNum = false)
        {
            var Data = GetSongData(config);
            if (Data is not CommonData.SongData SongData) return $"Unknown Song{(string.IsNullOrWhiteSpace(SongHash) ? "" : " [Error]")}";
            string Display = SongData.Name;
            if (WithArtist)
                Display += $" by {SongData.Artist}";
            if (WithAlbum)
                Display += $" from {SongData.Album}";
            if (WithSongNum)
                Display = $"[Song {SongNumber}] {Display}";
            return Display;
        }
        public CommonData.SongData? GetSongData(ConfigData config)
        {
            if (config is null) return null;
            if (SongHash is null) return null;
            if (!config.SongData.TryGetValue(SongHash, out var SongData)) return null;
            return SongData;
        }
        public bool HasStandardCheck(out long ID)
        {
            ID = APStandardCheckLocation ?? -1;
            return APStandardCheckLocation is not null;
        }
        public bool HasExtraCheck(out long ID)
        {
            ID = APExtraCheckLocation ?? -1;
            return APExtraCheckLocation is not null;
        }
        public bool HasFameCheck(out long ID)
        {
            ID = APFameCheckLocation ?? -1;
            return APFameCheckLocation is not null;
        }
        public bool StandardCheckAvailable(ConnectionData connection, out long StandardCheckID) =>
            HasStandardCheck(out StandardCheckID) && !connection.CheckedLocations.Contains(StandardCheckID);
        public bool ExtraCheckAvailable(ConnectionData connection, out long ExtraCheckID) =>
            HasExtraCheck(out ExtraCheckID) && !connection.CheckedLocations.Contains(ExtraCheckID);
        public bool FameCheckAvailable(ConnectionData connection, out long FameCheckID) => FameCheckAvailable(connection.CheckedLocations, out FameCheckID);
        public bool FameCheckAvailable(HashSet<long> CheckedLocations, out long FameCheckID)
        {
            if (!HasFameCheck(out FameCheckID)) return false;
            if (CheckedLocations.Contains(FameCheckID)) return false;
            bool standardComplete = !HasStandardCheck(out var sl) || CheckedLocations.Contains(sl);
            bool extraComplete = !HasExtraCheck(out var el) || CheckedLocations.Contains(el);
            return standardComplete && extraComplete;
        }
        public bool HasUncheckedLocations(ConnectionData connection) =>
            StandardCheckAvailable(connection, out _) || ExtraCheckAvailable(connection, out _) || FameCheckAvailable(connection, out _);
        public bool SongAvailableToPlay(ConnectionData connection, ConfigData config)
        {
            bool Available = IsGoalSong() ? connection.HasFamePointGoal(config) : SongItemReceived(connection);
            return Available && HasUncheckedLocations(connection);
        }
        public bool SongItemReceived(ConnectionData connection) => SongItemReceived(connection, out _);

        public bool SongItemReceived(ConnectionData connection, out APWorldData.BaseYargAPItem Data) => 
            connection.ReceivedSongs.TryGetValue(SongNumber, out Data);

        public bool IsGoalSong() => SongNumber == 0;
    }

    public static class SongHelper
    {
        public static string GetSongDisplayName(this SongData Data, bool WithArtist = true, bool WithAlbum = false)
        {
            string Display = Data.Name;
            if (WithArtist)
                Display += $" by {Data.Artist}";
            if (WithAlbum)
                Display += $" from {Data.Album}";
            return Display;
        }
    }
}
