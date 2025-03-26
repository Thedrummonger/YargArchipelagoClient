using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YargArchipelagoClient.Data;
using static YargArchipelagoClient.Helpers.MiscHelpers;

namespace YargArchipelagoClient.Helpers
{
    public static class MiscHelpers
    {
        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public class WeightedLimitedItem<T>(T pool, int available, int weight)
        {
            public T Pool { get; set; } = pool;
            public int Available { get; set; } = available;
            public int Weight { get; set; } = weight;
        }

        public static T GetWeightedLimitedItem<T>(this List<WeightedLimitedItem<T>> candidates, Random? random = null)
        {
            Random rng = random ?? new Random();
            var validCandidates = candidates.Where(c => c.Available > 0).ToList();
            if (validCandidates.Count < 1)
                throw new Exception("Ran out of valid candidates before all could be selected");
            int totalWeight = validCandidates.Sum(c => c.Weight);
            int randomValue = rng.Next(totalWeight);

            int cumulative = 0;
            WeightedLimitedItem<T>? selected = null;
            foreach (var candidate in validCandidates)
            {
                cumulative += candidate.Weight;
                if (randomValue < cumulative)
                {
                    selected = candidate;
                    break;
                }
            }

            if (selected is null)
                throw new Exception("Error while selecting song pool from random pools");

            selected.Available--;
            return selected.Pool;
        }
    }
}
