using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YargArchipelagoCore.Data
{
    public class PlandoData
    {
        public int SongNum { get; set; }
        public bool PoolPlandoEnabled { get; set; }
        public string? SongPool { get; set; }
        public bool SongPlandoEnabled { get; set; }
        public string? SongHash { get; set; }

        public bool HasValidPoolPlando => PoolPlandoEnabled && SongPool is not null;
        public bool HasValidSongPlando => SongPlandoEnabled && SongHash is not null;
        public bool HasValidPlando => HasValidPoolPlando || HasValidSongPlando;
    }
}
