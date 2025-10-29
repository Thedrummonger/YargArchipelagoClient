using TDMUtils;
using YargArchipelagoClient.Data;
using YargArchipelagoCommon;
using YargArchipelagoCore.Data;
using static YargArchipelagoCommon.CommonData;

namespace YargArchipelagoClient.Helpers
{
    public class SongPoolManager(List<SongPool> allPools, Dictionary<int, PlandoData> plandoSongData, ConfigData data)
    {
        Dictionary<string, HashSet<string>>? SongToPoolMap = null;
        /// <summary>
        /// Gets the total amount of songs assigned to this pool
        /// </summary>
        public int GetTotalAssignedToThisPool(SongPool pool) =>
            GetTotalConfigAssignedToThisPool(pool) + GetTotalPlandoAssignedToThisPool(pool);
        /// <summary>
        /// Gets the amount of songs applied to this pool via the config page
        /// </summary>
        public int GetTotalConfigAssignedToThisPool(SongPool pool) =>
            pool.RandomAmount ? 0 : pool.AmountInPool;
        /// <summary>
        /// Gets the amount of songs applied to this pool via plando
        /// </summary>
        public int GetTotalPlandoAssignedToThisPool(SongPool pool) =>
            plandoSongData.Values.Where(x => x.PoolPlandoEnabled && x.SongPool != null && x.SongPool == pool.Name).Count();
        /// <summary>
        /// Returns the overall number of songs assigned (config + plando) across all pools.
        /// </summary>
        public int GetOverallAssignedCount() =>
            allPools.Sum(GetTotalConfigAssignedToThisPool) + GetTotalLocationsHandledByPlando();
        /// <summary>
        /// Return the total amount of songs that could be assigned to all random pools
        /// </summary>
        public int GetOverallRandomAssignedCount() =>
            allPools.Sum(GetPotentialSongsForRandomPool);

        public int GetTotalLocationsHandledByPlando() =>
            plandoSongData.Values.Where(x => x.HasValidPlando).Count();

        /// <summary>
        /// Gets the amount of songs that could be applied to this pool when selecting it's random amount
        /// </summary>
        public int GetPotentialSongsForRandomPool(SongPool pool) =>
            pool.RandomAmount ? pool.GetAvailableSongs(data.SongData).Count - GetTotalAssignedToThisPool(pool) : 0;

        /// <summary>
        /// Gets the Total number of songs That could be applied to this pool
        /// </summary>
        public int GetTotalAmountAssignableToThisPool(SongPool pool)
        {
            var AvailablePerRestriction = pool.GetAvailableSongs(data.SongData).Count;
            var AvailablePerMaxSongs = data.TotalAPSongLocations - GetAmountAssignedFromOtherPools(pool);
            return Math.Min(AvailablePerMaxSongs, AvailablePerRestriction);
        }

        /// <summary>
        /// Gets the Total number of songs That could be applied to this pool via the config form
        /// </summary>
        /// <remarks>
        /// this is GetTotalAmountAssignableToThisPool minus GetTotalPlandoAssignedToThisPool. used to assign nud max value
        /// </remarks>
        public int GetTotalAmountAssignableToThisPoolViaConfig(SongPool pool) =>
            GetTotalAmountAssignableToThisPool(pool) - GetTotalPlandoAssignedToThisPool(pool);

        /// <summary>
        /// Gets the number of songs that could be assigned to a pool using the pools config
        /// </summary>
        public int GetRemainingAmountAssignableToThisPool(SongPool pool) =>
            GetTotalAmountAssignableToThisPool(pool) - GetTotalAssignedToThisPool(pool);

        public int GetAmountAssignedFromOtherPools(SongPool pool)
        {
            var Amount = 0;
            foreach(var otherPool in allPools)
            {
                if (otherPool.Name.Equals(pool.Name)) continue;
                Amount += GetTotalAssignedToThisPool(otherPool);
            }
            return Amount;
        }

        /// <summary>
        /// Gets the amount of songs assigned manually and handled by plando. 
        /// If this number falls short of the target and there are songs from random pools, 
        /// add those songs until we meet the target or run out.
        /// </summary>
        public int GetTotalPotentialSongAmount()
        {
            var AmmountAssigned = GetOverallAssignedCount();
            var AmountRandomAssignable = GetOverallRandomAssignedCount();

            // If we have manually assigned more location that are needed, return the amount we assigned (invalid)
            if (AmmountAssigned > data.TotalAPSongLocations)
                return AmmountAssigned;

            // At this point we know that AmmountAssigned <= totalAPSongLocations
            // If AmmountAssigned == totalAPSongLocations or can meet or exceed by adding AmountRandomAssignable, return the requirement amount (valid)
            if (AmmountAssigned + AmountRandomAssignable >= data.TotalAPSongLocations)
                return data.TotalAPSongLocations;

            //At this point we know we can not reach the required amount, return the max amount we can reach (invalid)
            return AmmountAssigned + AmountRandomAssignable;
        }

        /// <summary>
        /// Quick check to see if you are able to plando a pool to the given location
        /// </summary>
        public bool CanPlandoPoolToThisLocation(SongPool pool, PlandoData data)
        {
            if (data.PoolPlandoEnabled && data.SongPool != null && data.SongPool == pool.Name)
                return true; //Already plandoed to this pool, so we "can" plando it here
            return GetRemainingAmountAssignableToThisPool(pool) > 0;
        }

        public Dictionary<string, HashSet<string>> GetSongToPoolMap()
        {
            if (SongToPoolMap is not null) return SongToPoolMap;
            SongToPoolMap = [];
            foreach (var p in allPools)
            {
                var AvailableSongs = p.GetAvailableSongs(data.SongData).Values;
                foreach (var i in AvailableSongs)
                {
                    SongToPoolMap.SetIfEmpty(i.SongChecksum, []);
                    SongToPoolMap[i.SongChecksum].Add(p.Name);
                }
            }
            return SongToPoolMap;
        }
        public SongPool TryGetRandomUnusedPool(string SongHash, Dictionary<string, HashSet<string>> UsedSongs, ConnectionData connection)
        {
            int NonPlandoSongsAdded = allPools.Select(x => x.AmountInPool).Sum();
            var AllValidPools = GetSongToPoolMap()[SongHash].Select(x => allPools.First(y => y.Name == x));
            var FilteredPools = AllValidPools.Where(x => x.AmountInPool > 0 || NonPlandoSongsAdded == 0); //Try to not use pools that had no entries, unless no pool had entries
            FilteredPools = FilteredPools.Where(x => !UsedSongs.TryGetValue(x.Name, out var s) || !s.Contains(SongHash));
            if (!FilteredPools.Any())
                FilteredPools = [.. AllValidPools];

            var ValidPools = FilteredPools.ToArray();
            int randomIndex = connection.GetRNG().Next(ValidPools.Length);
            return ValidPools[randomIndex];
        }

        public SongData GetRandomUnusedSong(SongPool pool, Dictionary<string, HashSet<string>> UsedSongs, ConnectionData connection)
        {
            var availableSongs = pool.GetAvailableSongs(data.SongData, UsedSongs).Values.ToArray();
            int randomIndex = connection.GetRNG().Next(availableSongs.Length);
            return availableSongs[randomIndex]; ;
        }

        public HashSet<SongData> GetValidSongsForAllPools()
        {
            HashSet<CommonData.SongData> ValidSongs = [];
            foreach (var p in allPools)
            {
                foreach (var s in p.GetAvailableSongs(data.SongData).Values)
                {
                    ValidSongs.Add(s);
                }
            }
            return ValidSongs;
        }
        public bool CanPlandoAnyPoolToThisLocation(PlandoData selectedLocation, out IEnumerable<SongPool> validPools)
        {
            validPools = allPools.Where(x => CanPlandoPoolToThisLocation(x, selectedLocation));
            return validPools.Any();
        }

        public bool CanPlandoSongToThisLocation(PlandoData selectedSong)
        {
            return GetOverallAssignedCount() < data.TotalAPSongLocations || selectedSong.HasValidPlando;
        }

        public string SongDisplay(int num) => num > 0 ? $"Song {num}" : "Goal Song";

    }
}
