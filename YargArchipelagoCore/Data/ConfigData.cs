using Archipelago.MultiClient.Net;
using TDMUtils;
using YargArchipelagoCommon;
using static YargArchipelagoCommon.CommonData;

namespace YargArchipelagoCore.Data
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

        public bool InGameAPChat = true;
        public ItemLog InGameItemLog = ItemLog.ToMe;

        public bool ManualMode = false;

        public bool CheatMode = false;

        public int FamePointsNeeded = 0;

        [Newtonsoft.Json.JsonIgnore]
        public bool DebugPrintAllSongs = false;

        [Newtonsoft.Json.JsonIgnore]
        public UserConfig? CurrentUserConfig;

        /// <summary>
        /// This value tracks if deathlink was enabled in the YAML. it will never change
        /// </summary>
        public bool ServerDeathLink = false;

        /// <summary>
        /// This value is a togglable deathlink override. If ServerDeathLink is enabled, this can be disabled to prevent Deathlinks.
        /// </summary>
        public bool deathLinkEnabled = false;

        public Dictionary<APWorldData.StaticItems, int> ProcessedTrapsFiller = [];

        public int TotalAPSongLocations => GetSongIndexes().Length;

        public int[] GetSongIndexes() => [0, .. ApLocationData.Keys];

        public SongLocation GetSongLocation(int SongNumber)
        {
            if (SongNumber == GoalSong.SongNumber)
                return GoalSong;
            else if (ApLocationData.TryGetValue(SongNumber, out var songLocation))
                return songLocation;
            throw new Exception($"{SongNumber} was not a valid song location");
        }

        public void SaveConfigFile(ConnectionData Connection)
        {
            File.WriteAllText(Path.Combine(SeedConfigPath, Connection.getSaveFileName()), this.ToFormattedJson());
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
        public CommonData.SongData[] GetUnusableSongs()
        {
            HashSet<string> ValidSongs = [];
            var AllProfiles = ApLocationData.Values.DistinctBy(i => i.Requirements!.Name).Select(i => i.Requirements);
            foreach (var profile in AllProfiles)
            {
                var validForProfile = profile!.GetAvailableSongs(SongData);
                foreach (var item in validForProfile)
                    ValidSongs.Add(item.Key);
            }
            return [.. SongData.Where(x => !ValidSongs.Contains(x.Key)).Select(x => x.Value)];
        }
    }
}
