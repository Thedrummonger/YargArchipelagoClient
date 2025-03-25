using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YargArchipelagoClient.Data;
using static YargArchipelagoClient.Forms.PlandoForm;
using static YargArchipelagoCommon.CommonData;

namespace YargArchipelagoClient.Helpers
{
    public class SongPoolManager(List<SongPool> allPools, Dictionary<int, PlandoData> plandoSongData, int totalAPSongLocations,Dictionary<string, SongData> allSongData)
    {

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
            allPools.Sum(GetTotalAssignedToThisPool);
        /// <summary>
        /// Return the total amount of songs that could be assigned to all random pools
        /// </summary>
        public int GetOverallRandomAssignedCount() =>
            allPools.Sum(GetPotentialSongsForRandomPool);

        /// <summary>
        /// Gets the amount of songs that could be applied to this pool when selecting it's random amount
        /// </summary>
        public int GetPotentialSongsForRandomPool(SongPool pool) =>
            pool.RandomAmount ? GetRemainingAmountAssignableToThisPool(pool) : 0;

        /// <summary>
        /// Gets the Total number of songs That could be applied to this pool
        /// </summary>
        public int GetTotalAmountAssignableToThisPool(SongPool pool)
        {
            var AvailablePerRestriction = pool.GetAvailableSongs(allSongData).Count;
            var AvailablePerMaxSongs = totalAPSongLocations - GetAmountAssignedFromOtherPools(pool);
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
        /// Gets the amount of 
        /// </summary>
        public int GetOverallAssignedCountForLabel()
        {
            var AmmountAssigned = GetOverallAssignedCount();
            var AmountRandomAssignable = GetOverallRandomAssignedCount();
            //if we have assigned enough that we don't need to use random pools, or if there are no random pools, show the amount assigned
            if (AmmountAssigned >= totalAPSongLocations || AmountRandomAssignable < 1)
                return AmmountAssigned;
            //if our total potential count is less that what is needed, show the total potential count
            if (AmmountAssigned + AmountRandomAssignable < totalAPSongLocations)
                return AmmountAssigned + AmountRandomAssignable;
            //This should only happen if statically assigned amount is qual to our needed amount
            //or statically assigned amount is less than the needed amount but we have enough random assignable to cover it
            //We are good to go, so just return the needed amount
            return totalAPSongLocations;
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
    }
}
