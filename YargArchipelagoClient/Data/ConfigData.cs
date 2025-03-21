using Archipelago.MultiClient.Net;
using TDMUtils;
using YargArchipelagoCommon;

namespace YargArchipelagoClient.Data
{
    public class ConfigData()
    {
        /// <summary>
        /// A list of songs and their data pulled from the given song folder
        /// </summary>
        public Dictionary<string, CommonData.SongData> SongData { get; set; } = [];
        /// <summary>
        /// A mapping of the song "number" to it's AP data and mapped song
        /// </summary>
        public Dictionary<int, SongLocation> ApLocationData { get; } = [];
        /// <summary>
        /// The goal song info
        /// </summary>
        public SongLocation GoalSong = new(0);

        public Dictionary<APWorldData.StaticItems, int> UsedFiller = [];

        public bool BroadcastSongName = false;

        public bool ManualMode = false;

        public int FamePointsNeeded = 0;

        public bool deathLinkEnabled = false;

        public Dictionary<APWorldData.StaticItems, int> TrapsRegistered = [];

        public int TotalSongsInPool => ApLocationData.Count + 1; // +1 For Goal Song

        public int[] GetSongIndexes() => [0, .. ApLocationData.Keys];

        public void SaveConfigFile(ConnectionData Connection)
        {
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seeds", Connection.getSaveFileName()), this.ToFormattedJson());
        }
        public void ParseAPLocations(ArchipelagoSession archipelagoSession)
        {
            var Locations = APWorldData.APIDs.StaticLocationIDs;
            var SongLocations = APWorldData.APIDs.SongLocationIDs;
            foreach (var i in archipelagoSession.Locations.AllLocations)
            {
                if (Locations.TryGetValue(i, out var Location))
                {
                    if (Location == APWorldData.StaticLocations.Goal)
                    {
                        GoalSong.APStandardCheckLocation = i;
                        continue;
                    }
                }
                if (SongLocations.TryGetValue(i, out var Song))
                {
                    ApLocationData.SetIfEmpty(Song.songnum, new(Song.songnum));
                    switch (Song.locType)
                    {
                        case APWorldData.LocationType.standard:
                            ApLocationData[Song.songnum].APStandardCheckLocation = i;
                            continue;
                        case APWorldData.LocationType.extra:
                            ApLocationData[Song.songnum].APExtraCheckLocation = i;
                            continue;
                        case APWorldData.LocationType.fame:
                            ApLocationData[Song.songnum].APFameCheckLocation = i;
                            continue;
                    }
                }
                throw new Exception($"{i} was not a valid AP id [{archipelagoSession.Locations.GetLocationNameFromId(i)}]");
            }
        }

        public SongLocation[] GetAllSongLocations() => [GoalSong, .. ApLocationData.Values];
    }
}
