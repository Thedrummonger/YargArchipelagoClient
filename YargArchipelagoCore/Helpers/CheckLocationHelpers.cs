using TDMUtils;
using YargArchipelagoCommon;
using YargArchipelagoCore.Data;

namespace YargArchipelagoCore.Helpers
{
    public static class CheckLocationHelpers
    {
        public static bool HasFamePointGoal(this ConnectionData Connection, ConfigData Config) =>
            GetCurrentFame(Connection) >= Config.FamePointsNeeded;
        public static int GetCurrentFame(this ConnectionData Connection) =>
            Connection.ApItemsRecieved.Where(x => x.Type == APWorldData.StaticItems.FamePoint).Count();

        public static void CheckLocations(ConfigData Config, ConnectionData Connection, CommonData.SongCompletedData passInfo)
        {
            HashSet<long> ToCheck = [];
            HashSet<SongLocation> AlteredLocations = [];
            foreach (var Target in Config.GetAllSongLocations())
            {
                if (Target.SongHash != passInfo!.SongData.SongChecksum)
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
                    else if (Config.DeathLinkMode > CommonData.DeathLinkType.None && SL1DL)
                        Connection.DeathLinkService!.SendDeathLink(new(Connection.SlotName, $"{Connection.SlotName} failed song {Target.GetSongDisplayName(Config!)}"));
                }
                if (Target.ExtraCheckAvailable(Connection, out var EL1))
                {
                    if (Target.Requirements!.MetExtra(passInfo, out var EL1DL))
                    {
                        ToCheck.Add(EL1);
                        AlteredLocations.Add(Target);
                    }
                    else if (Config.DeathLinkMode > CommonData.DeathLinkType.None && EL1DL)
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

            SendEnergy(Connection, Config, passInfo.BandScore, ToCheck.Count > 0);
        }

        public static void CommitCheckLocations(this ConnectionData Connection, IEnumerable<long> Locations, IEnumerable<SongLocation> songLocations, ConfigData Config)
        {
            if (Config!.BroadcastSongName)
            {
                //foreach (var i in songLocations)
                //    Connection.GetSession().Say(i.GetSongDisplayName(Config!, true, true, true));
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

        const long minScale = 20000;
        const long maxScale = 1000000;
        public static void SendEnergy(ConnectionData connection, ConfigData config, long amount, bool WasLocationChecked)
        {
            //if (!config.energylink) return //TODO add setting

            var Session = connection.GetSession();

            int AmountOfLocationsTotal = connection.GetSession().Locations.AllLocations.Count;
            int AmountOfLocationsChecked = connection.GetSession().Locations.AllLocationsChecked.Count;
            double completionPercentage = AmountOfLocationsChecked / AmountOfLocationsTotal;

            double scale = minScale + (completionPercentage * (maxScale - minScale));

            long Energy = (long)(amount * scale);

            //Save this, if I don't inherit newtonsoft from multiclient everything breaks.
            //If I ever need to use my own version this is how I will need to initialize datastore values
            /*
            dynamic dataStorage = Session.DataStorage[EnergyLinkKey];
            dynamic token = Newtonsoft.Json.Linq.JToken.FromObject(0);
            dataStorage.Initialize(token);
            */

            string EnergyLinkKey = $"EnergyLink{Session.Players.ActivePlayer.Team}";
            Session.DataStorage[EnergyLinkKey].Initialize(0);
            Session.DataStorage[EnergyLinkKey] += Energy;
        }

        public static long GetEnergy(ConnectionData connection)
        {
            var Session = connection.GetSession();
            string EnergyLinkKey = $"EnergyLink{Session.Players.ActivePlayer.Team}";
            Session.DataStorage[EnergyLinkKey].Initialize(0);
            return Session.DataStorage[EnergyLinkKey];
        }

        public static bool SpendEnergy(ConnectionData connection, long Amount)
        {
            var Session = connection.GetSession();
            string EnergyLinkKey = $"EnergyLink{Session.Players.ActivePlayer.Team}";
            Session.DataStorage[EnergyLinkKey].Initialize(0);
            if (Session.DataStorage[EnergyLinkKey] >= Amount)
            {
                Session.DataStorage[EnergyLinkKey] -= Amount;
                return true;
            }
            return false;

        }
    }
}
