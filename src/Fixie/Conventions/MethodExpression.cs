namespace Fixie.Conventions
{
    using System;
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
        /// Limits discovered test methods to those methods which have the specified attribute.
        /// </summary>
        public MethodExpression Has<TAttribute>() where TAttribute : Attribute
        {
            return Where(method => method.Has<TAttribute>());
        }

        /// <summary>
        /// Limits discovered test methods to those methods which have or inherit the specified attribute.
        /// </summary>
        public MethodExpression HasOrInherits<TAttribute>() where TAttribute : Attribute
        {
            return Where(method => method.HasOrInherits<TAttribute>());
        }

        /// <summary>
        /// Randomizes the order of execution of a test class's contained test methods, using the
        /// given pseudo-random number generator.
        /// </summary>
        public MethodExpression ShuffleMethods(Random random)
        {
            config.OrderMethods = methods => Shuffle(methods, random);
            return this;
        }

        /// <summary>
        /// Randomizes the order of execution of a test class's contained test methods.
        /// </summary>
        public MethodExpression ShuffleMethods()
        {
            return ShuffleMethods(new Random());
        }

        /// <summary>
        /// Defines the order of execution of a test class's contained test methods.
        /// </summary>
        public MethodExpression SortMethods(Comparison<MethodInfo> comparison)
        {
            config.OrderMethods = methods => Array.Sort(methods, comparison);
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