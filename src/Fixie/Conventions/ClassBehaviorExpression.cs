namespace Fixie.Conventions
{
    using System;
    using System.Reflection;

    public class ClassBehaviorExpression
    {
        readonly Configuration config;

        internal ClassBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Overrides the default test class lifecycle.
        /// </summary>
        public ClassBehaviorExpression Lifecycle<TLifecycle>() where TLifecycle : Lifecycle
        {
            config.Lifecycle = (Lifecycle)Activator.CreateInstance(typeof(TLifecycle));
            return this;
        }

        /// <summary>
        /// Overrides the default test class lifecycle.
        /// </summary>
        public ClassBehaviorExpression Lifecycle(Lifecycle lifecycle)
        {
            config.Lifecycle = lifecycle;
            return this;
        }

        /// <summary>
        /// Randomizes the order of execution of a test class's contained test methods, using the
        /// given pseudo-random number generator.
        /// </summary>
        public ClassBehaviorExpression ShuffleMethods(Random random)
        {
            config.OrderMethods = methods => Shuffle(methods, random);
            return this;
        }

        /// <summary>
        /// Randomizes the order of execution of a test class's contained test methods.
        /// </summary>
        public ClassBehaviorExpression ShuffleMethods()
        {
            return ShuffleMethods(new Random());
        }

        /// <summary>
        /// Defines the order of execution of a test class's contained test methods.
        /// </summary>
        public ClassBehaviorExpression SortMethods(Comparison<MethodInfo> comparison)
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