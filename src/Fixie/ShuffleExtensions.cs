namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ShuffleExtensions
    {
        /// <summary>
        /// Randomizes the order of the given tests.
        /// </summary>
        public static IReadOnlyList<TestMethod> Shuffle(this IEnumerable<TestMethod> tests)
        {
            return tests.Shuffle(new Random());
        }

        /// <summary>
        /// Randomizes the order of the given tests, using the given pseudo-random number generator.
        /// </summary>
        public static IReadOnlyList<TestMethod> Shuffle(this IEnumerable<TestMethod> tests, Random random)
        {
            var array = tests.ToArray();

            FisherYatesShuffle(array, random);

            return array;
        }

        static void FisherYatesShuffle<T>(T[] array, Random random)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = random.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}