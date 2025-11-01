using YargArchipelagoCommon;

namespace YargArchipelagoCore.Data
{
    public class PlandoData
    {
        public int SongNum { get; set; }
        public bool PoolPlandoEnabled { get; set; }
        public string? SongPool { get; set; }
        public bool SongPlandoEnabled { get; set; }
        public string? SongHash { get; set; }

        public bool HasValidPoolPlando => PoolPlandoEnabled && SongPool is not null;
        public bool HasValidSongPlando => SongPlandoEnabled && SongHash is not null;
        public bool HasValidPlando => HasValidPoolPlando || HasValidSongPlando;

        public SongPool? GetCurrentPool(List<SongPool> songPools)
        {
            if (!HasValidPoolPlando) return null;
            return songPools.FirstOrDefault(x => x.Name == SongPool);
        }
        public CommonData.SongData? GetCurrentSong(ConfigData data)
        {
            if (!HasValidSongPlando) return null;
            return data.SongData.TryGetValue(SongHash!, out var Song) ? Song : null;
        }
        public HashSet<CommonData.SongData> ValidSongsForThisPlando(List<SongPool> songPools, ConfigData data)
        {
            if (!HasValidPoolPlando) return songPools.GetValidSongsForAllPools(data);
            var CurrentPool = GetCurrentPool(songPools);
            if (CurrentPool is null) return [];
            return [..CurrentPool.GetAvailableSongs(data.SongData).Values.Where(x => x.Difficulties.ContainsKey(CurrentPool.Instrument))];
        }
        public HashSet<SongPool> ValidPoolsForThisPlando(List<SongPool> songPools, ConfigData data)
        {
            if (!HasValidSongPlando) return [..songPools];
            var CurrentSong = GetCurrentSong(data);
            if (CurrentSong is null) return [];
            return [.. songPools.Where(x => CurrentSong.Difficulties.ContainsKey(x.Instrument))];
        }
    }
}
