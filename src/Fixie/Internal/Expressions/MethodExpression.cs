namespace Fixie.Internal.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class MethodExpression
    {
        readonly Configuration config;

        internal MethodExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Limits discovered test methods to those which satisfy the given condition.
        /// Multiple calls to this method establish multiple conditions, all of which
        /// must be satisfied for a method to be considered a test method.
        /// </summary>
        public MethodExpression Where(Func<MethodInfo, bool> condition)
        {
            config.AddTestMethodCondition(condition);
            return this;
        }

        /// <summary>
        /// Randomizes the order of execution of a test class's contained test methods, using the
        /// given pseudo-random number generator.
        /// </summary>
        public MethodExpression Shuffle(Random random)
        {
            config.OrderMethods = methods =>
            {
                var array = methods.ToArray();

                Shuffle(array, random);

                return array;
            };
            return this;
        }

        /// <summary>
        /// Randomizes the order of execution of a test class's contained test methods.
        /// </summary>
        public MethodExpression Shuffle()
        {
            return Shuffle(new Random());
        }

        /// <summary>
        /// Defines the order of execution of a test class's contained test methods.
        /// </summary>
        public MethodExpression OrderBy<TKey>(Func<MethodInfo, TKey> keySelector)
        {
            config.OrderMethods = methods => methods.OrderBy(keySelector).ToList();
            return this;
        }

        /// <summary>
        /// Defines the order of execution of a test class's contained test methods.
        /// </summary>
        public MethodExpression OrderBy<TKey>(Func<MethodInfo, TKey> keySelector, IComparer<TKey> comparer)
        {
            config.OrderMethods = methods => methods.OrderBy(keySelector, comparer).ToList();
            return this;
        }

        /// <summary>
        /// Defines the order of execution of a test class's contained test methods.
        /// </summary>
        public MethodExpression OrderByDescending<TKey>(Func<MethodInfo, TKey> keySelector)
        {
            config.OrderMethods = methods => methods.OrderByDescending(keySelector).ToList();
            return this;
        }

        /// <summary>
        /// Defines the order of execution of a test class's contained test methods.
        /// </summary>
        public MethodExpression OrderByDescending<TKey>(Func<MethodInfo, TKey> keySelector, IComparer<TKey> comparer)
        {
            config.OrderMethods = methods => methods.OrderByDescending(keySelector, comparer).ToList();
            return this;
        }

        //Fisher-Yates Shuffle
        //  C# implementation from http://stackoverflow.com/a/110570
        static void Shuffle<T>(T[] array, Random random)
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