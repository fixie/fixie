using System;

namespace Fixie
{
    public static class ArrayExtensions
    {
        //Fisher-Yates Shuffle
        //  C# implementation from http://stackoverflow.com/a/110570
        public static void Shuffle<T>(this T[] array, Random random)
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