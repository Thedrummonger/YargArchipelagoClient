using Archipelago.MultiClient.Net;

namespace ArchipelagoPowerTools.Data
{
    public class ConnectionData(string? address, string slotname, string password, ArchipelagoSession session)
    {
        private ArchipelagoSession Session = session;
        public string? Address { get; private set; } = address;
        public string? SlotName { get; private set; } = slotname;
        public string? Password { get; private set; } = password;
        public ArchipelagoSession GetSession() => Session;
        public void SetSession(ArchipelagoSession session)
        {
            if (Session is not null) throw new Exception("Session has already been initialized");
            Session = session;
        }
        private IEnumerable<long>? _locations = null;
        public IEnumerable<long> GetLocations()
        {
            _locations ??= GetSession().Locations.AllLocations;
            return _locations;
        }
    }
}
