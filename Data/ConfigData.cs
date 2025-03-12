using Archipelago.MultiClient.Net;
using TDMUtils;

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

        public Dictionary<CommonData.StaticItems, int> UsedFiller = [];

        public bool BroadcastSongName = false;

        public bool ManualMode = false;

        public int FamePointsNeeded = 0;

        public bool deathLinkEnabled = false;

        public int TotalSongsInPool => ApLocationData.Count + 1; // +1 For Goal Song

        public void ParseAPLocations(ArchipelagoSession archipelagoSession)
        {
            var Locations = CommonData.APIDs.Locations;
            var SongLocations = CommonData.APIDs.SongLocationIDs;
            foreach (var i in archipelagoSession.Locations.AllLocations)
            {
                if (Locations.TryGetValue(i, out var Location))
                {
                    if (Location == CommonData.StaticLocations.Goal)
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
                        case CommonData.LocationType.standard:
                            ApLocationData[Song.songnum].APStandardCheckLocation = i;
                            continue;
                        case CommonData.LocationType.extra:
                            ApLocationData[Song.songnum].APExtraCheckLocation = i;
                            continue;
                        case CommonData.LocationType.fame:
                            ApLocationData[Song.songnum].APFameCheckLocation = i;
                            continue;
                    }
                }
                throw new Exception($"{i} was not a valid AP id [{archipelagoSession.Locations.GetLocationNameFromId(i)}]");
            }
        }
    }
}
