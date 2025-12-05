using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static AsitLib.StringHelpers;

namespace AsitLib.Chance
{
    public static class RandomExtensions
    {
        public static bool NextBool(this Random rand, double chance = 0.5)
        {
            return rand.NextDouble() < chance;
        }

        public static T PickFrom<T>(this Random rand, IEnumerable<T> values)
            => values is IReadOnlyList<T> list ? list[rand.Next(list.Count)] : values.OrderBy(x => rand.Next()).First();

        public static T PickFrom<T>(this Random random, IReadOnlyDictionary<T, int> values)
        {
            int randomValue = random.Next(values.Values.Sum());
            int cumulativeWeight = 0;

            foreach ((T k, int v) in values)
            {
                cumulativeWeight += v;
                if (randomValue < cumulativeWeight)
                {
                    return k;
                }
            }

            throw new InvalidOperationException("Unexpected error in weighted random selection.");
        }
    }
}
