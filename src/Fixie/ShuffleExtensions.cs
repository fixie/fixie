using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie;

public static class ShuffleExtensions
{
    /// <summary>
    /// Randomizes the order of the given items.
    /// </summary>
    public static IReadOnlyList<T> Shuffle<T>(this IEnumerable<T> items)
    {
        return items.Shuffle(new Random());
    }

    /// <summary>
    /// Randomizes the order of the given items, using the given pseudo-random number generator.
    /// </summary>
    public static IReadOnlyList<T> Shuffle<T>(this IEnumerable<T> items, Random random)
    {
        var array = items.ToList();

        FisherYatesShuffle(array, random);

        return array;
    }

    static void FisherYatesShuffle<T>(IList<T> array, Random random)
    {
        int n = array.Count;
        while (n > 1)
        {
            int k = random.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
}