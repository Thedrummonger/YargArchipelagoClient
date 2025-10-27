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
    public class ConfigCreator
    {
        public bool HasErrored = true;
        public ConfigCreator(ConnectionData connection)
        {
            Connection = connection;
            data = new ConfigData();
            data.ParseAPLocations(connection.GetSession());
            PlandoSongData = data.GetSongIndexes().ToDictionary(x => x, x => new PlandoData { SongNum = x });
            if (!SongImporter.TryReadSongs(out var SongData)) { HasErrored = true; return; }
            data.SongData = SongData;
            SongPoolManager = new(Pools, PlandoSongData, data.TotalAPSongLocations, data.SongData);
        }
        public ConfigData data;
        public ConnectionData Connection;
        public readonly List<SongPool> Pools = [];
        public readonly Dictionary<int, PlandoData> PlandoSongData;
        public SongPoolManager SongPoolManager;
    }
}
