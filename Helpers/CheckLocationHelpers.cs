using ArchipelagoPowerTools.Data;
using Newtonsoft.Json;
using System.Diagnostics;
using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Forms;

namespace YargArchipelagoClient.Helpers
{
    public static class CheckLocationHelpers
    {
        public static bool HasFamePointGoal(this ConnectionData Connection, ConfigData Config) =>
            GetCurrentFame(Connection) >= Config.FamePointsNeeded;
        public static int GetCurrentFame(this ConnectionData Connection) =>
            Connection.ReceivedFiller.TryGetValue(CommonData.StaticItems.FamePoint, out var famePoints) ? famePoints : 0;

        private static string LastReadContent = string.Empty;
        public static void CheckLocations(ConfigData Config, ConnectionData Connection)
        {
            try
            {
                if (!Directory.Exists(CommonData.DataFolder) || !File.Exists(CommonData.LastPlayedSong)) return;
                using var stream = File.Open(CommonData.LastPlayedSong, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                string newContent = reader.ReadToEnd();
                if (newContent == LastReadContent) return; // if unchanged, do nothing
                LastReadContent = newContent;

                var passInfo = JsonConvert.DeserializeObject<CommonData.SongPassInfo>(newContent);
                if (passInfo is null) return;
                var TargetSong = Config!.ApLocationData.Values.FirstOrDefault(x => x.SongHash == passInfo!.SongHash);
                if (TargetSong is null)
                {
                    if (Config!.GoalSong.SongHash == passInfo!.SongHash)
                        TargetSong = Config!.GoalSong;
                    else
                        return;
                }

                bool IsAvailableSong = TargetSong.SongAvailableToPlay(Connection);
                bool IsAvailableGoal = TargetSong.IsGoalSong(Config) && Connection.HasFamePointGoal(Config);
                if (!IsAvailableSong && !IsAvailableGoal) return;

                HashSet<long> ToCheck = [];
                if (TargetSong.StandardCheckAvailable(Connection, out var SL1))
                {
                    if (TargetSong.Requirements!.MetStandard(passInfo, out var SL1DL))
                        ToCheck.Add(SL1);
                    else if (Config.deathLinkEnabled && SL1DL)
                        Connection.DeathLinkService.SendDeathLink(new(Connection.SlotName, $"Failed {TargetSong.GetSongDisplayName(Config!)}"));
                }
                if (TargetSong.ExtraCheckAvailable(Connection, out var EL1))
                {
                    if (TargetSong.Requirements!.MetExtra(passInfo, out var EL1DL))
                        ToCheck.Add(EL1);
                    else if (Config.deathLinkEnabled && EL1DL)
                        Connection.DeathLinkService.SendDeathLink(new(Connection.SlotName, $"Failed {TargetSong.GetSongDisplayName(Config!)}"));
                }
                if (TargetSong.FameCheckAvailable([.. Connection.CheckedLocations, .. ToCheck], out var FL2))
                    ToCheck.Add(FL2);

                if (ToCheck.Count > 0)
                    Connection.CommitCheckLocations(ToCheck, [TargetSong], Config);
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"Failed to read Last Played Song {ex}");
            }
        }

        public static void CheckLocations(ConfigData Config, ConnectionData Connection, IEnumerable<SongLocation> locations, bool SkipValidation)
        {
            if (!Config!.ManualMode) return;
            var locationIDs = new HashSet<long>();
            var CheckStateChanged = new HashSet<SongLocation>();

            foreach (var songLocation in locations)
            {

                if (songLocation.FameCheckAvailable(Connection, out var fl))
                {
                    locationIDs.Add(fl);
                    continue;
                }

                List<long> ToCheck = [];
                var buttons = new List<CustomMessageResult>();
                int btnCheckCount = 0;
                if (songLocation.StandardCheckAvailable(Connection, out _))
                {
                    btnCheckCount++;
                    buttons.Add(CustomMessageResult.Reward1);
                }
                if (songLocation.ExtraCheckAvailable(Connection, out _))
                {
                    btnCheckCount++;
                    buttons.Add(CustomMessageResult.Reward2);
                }
                if (btnCheckCount > 1)
                    buttons.Add(CustomMessageResult.Both);

                var result = SkipValidation ?
                    CustomMessageResult.Both :
                    APSongMessageBox.Show(
                    $"Check Song {songLocation.GetSongDisplayName(Config!, false, false, true)}",
                    songLocation.GetSongDisplayName(Config!, true, true, false),
                    [.. buttons]);

                if (result.In(CustomMessageResult.Reward1, CustomMessageResult.Both) && songLocation.StandardCheckAvailable(Connection, out var sl1))
                    ToCheck.Add(sl1);
                if (result.In(CustomMessageResult.Reward2, CustomMessageResult.Both) && songLocation.ExtraCheckAvailable(Connection, out var el1))
                    ToCheck.Add(el1);
                if (songLocation.FameCheckAvailable([.. Connection.CheckedLocations, .. ToCheck], out var fl2))
                    ToCheck.Add(fl2);

                if (ToCheck.Count > 0) CheckStateChanged.Add(songLocation);
                locationIDs = [.. locationIDs, .. ToCheck];
            }
            Connection.CommitCheckLocations(locationIDs, CheckStateChanged, Config);
        }

        public static void CommitCheckLocations(this ConnectionData Connection, IEnumerable<long> Locations, IEnumerable<SongLocation> songLocations, ConfigData Config)
        {
            if (Config!.BroadcastSongName)
            {
                foreach (var i in songLocations)
                    Connection.GetSession().Say(i.GetSongDisplayName(Config!, true, true, true));
            }
            Connection!.GetSession().Locations.CompleteLocationChecks([.. Locations]);
            Connection!.UpdateCheckedLocations();
        }
    }
}
