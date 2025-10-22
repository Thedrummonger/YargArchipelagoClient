using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Forms;
using YargArchipelagoCommon;

namespace YargArchipelagoClient.Helpers
{
    public static class CheckLocationHelpers
    {
        public static bool HasFamePointGoal(this ConnectionData Connection, ConfigData Config) =>
            GetCurrentFame(Connection) >= Config.FamePointsNeeded;
        public static int GetCurrentFame(this ConnectionData Connection) =>
            Connection.ReceivedStaticItems.TryGetValue(APWorldData.StaticItems.FamePoint, out var famePoints) ? famePoints : 0;

        public static void CheckLocations(ConfigData Config, ConnectionData Connection, CommonData.SongCompletedData passInfo)
        {
            HashSet<long> ToCheck = [];
            HashSet<SongLocation> AlteredLocations = [];
            foreach (var Target in Config.GetAllSongLocations())
            {
                if (Target.SongHash != passInfo!.songData.SongChecksum)
                    continue;

                if (!Target.SongAvailableToPlay(Connection, Config))
                    continue;

                if (Target.StandardCheckAvailable(Connection, out var SL1))
                {
                    if (Target.Requirements!.MetStandard(passInfo, out var SL1DL))
                    {
                        ToCheck.Add(SL1);
                        AlteredLocations.Add(Target);
                    }
                    else if (Config.deathLinkEnabled && SL1DL)
                        Connection.DeathLinkService!.SendDeathLink(new(Connection.SlotName, $"{Connection.SlotName} failed song {Target.GetSongDisplayName(Config!)}"));
                }
                if (Target.ExtraCheckAvailable(Connection, out var EL1))
                {
                    if (Target.Requirements!.MetExtra(passInfo, out var EL1DL))
                    {
                        ToCheck.Add(EL1);
                        AlteredLocations.Add(Target);
                    }
                    else if (Config.deathLinkEnabled && EL1DL)
                        Connection.DeathLinkService!.SendDeathLink(new(Connection.SlotName, $"{Connection.SlotName} failed song {Target.GetSongDisplayName(Config!)}"));
                }
                if (Target.FameCheckAvailable([.. Connection.CheckedLocations, .. ToCheck], out var FL2))
                {
                    ToCheck.Add(FL2);
                    AlteredLocations.Add(Target);
                }
            }

            if (ToCheck.Count > 0)
                Connection.CommitCheckLocations(ToCheck, AlteredLocations, Config);
        }

        public static void CheckLocations(ConfigData Config, ConnectionData Connection, IEnumerable<SongLocation> locations, bool SkipConfirmPrompt)
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

                var result = SkipConfirmPrompt ?
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
            SendAvailableSongUpdate(Config, Connection);
        }

        public static void SendAvailableSongUpdate(ConfigData Config, ConnectionData Connection)
        {
            _ = Connection.GetPacketServer()?.SendPacketAsync(new CommonData.Networking.YargAPPacket
            {
                AvailableSongs = [.. Config.GetAllSongLocations().Where(x => 
                    x.SongHash is not null && 
                    x.Requirements is not null && 
                    x.SongAvailableToPlay(Connection, Config)).Select(x => (x.SongHash!, x.Requirements!.Name))]
            });
        }

    }
}
