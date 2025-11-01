using TDMUtils;
using YargArchipelagoCore.Data;
using YargArchipelagoCommon;

namespace YargArchipelagoCore.Helpers
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

        public static void CommitCheckLocations(this ConnectionData Connection, IEnumerable<long> Locations, IEnumerable<SongLocation> songLocations, ConfigData Config)
        {
            if (Config!.BroadcastSongName)
            {
                foreach (var i in songLocations)
                    Connection.GetSession().Say(i.GetSongDisplayName(Config!, true, true, true));
            }
            Connection!.GetSession().Locations.CompleteLocationChecks([.. Locations]);
            Connection.GetPacketServer().SendClientStatusPacket();
        }

        public static SongLocation[] GetAllAvailableSongLocations(this ConnectionData connection, ConfigData config, bool respectCheatMode = true)
        {
            if (config.CheatMode && respectCheatMode)
                return [config.GoalSong, .. config!.ApLocationData.Values.OrderBy(x => x.SongNumber)];

            List<SongLocation> songLocations = [];

            if (config.GoalSong.SongAvailableToPlay(connection, config))
                songLocations.Add(config.GoalSong);
            foreach (var i in config!.ApLocationData.OrderBy(x => x.Key))
                if (i.Value.SongAvailableToPlay(connection, config))
                    songLocations.Add(i.Value);
            return [..songLocations];
        }

    }
}
