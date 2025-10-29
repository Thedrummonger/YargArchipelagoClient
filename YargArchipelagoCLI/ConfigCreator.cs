using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YargArchipelagoClient.Data;
using YargArchipelagoClient.Helpers;
using YargArchipelagoCore.Data;
using static YargArchipelagoCore.Helpers.MultiplatformHelpers;

namespace YargArchipelagoCLI
{
    public class ConfigCreator(ConnectionData connection)
    {
        private ConfigData data = new();
        private ConnectionData Connection;
        private readonly List<SongPool> Pools = [];
        private Dictionary<int, PlandoData> PlandoSongData;
        private SongPoolManager SongPoolManager;

        public ConfigData? CreateConfig()
        {
            data.ParseAPLocations(Connection.GetSession());
            if (!SongImporter.TryReadSongs(out var SongData)) { return null; }
            data.SongData = SongData;
            PlandoSongData = data.GetSongIndexes().ToDictionary(x => x, x => new PlandoData { SongNum = x });
            SongPoolManager = new(Pools, PlandoSongData, data.TotalAPSongLocations, data.SongData);
            return null;
        }
    }
}
