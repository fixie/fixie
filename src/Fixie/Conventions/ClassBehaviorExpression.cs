namespace Fixie.Conventions
{
    using System;

    public class ClassBehaviorExpression
    {
        readonly Configuration config;

        internal ClassBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        public ClassBehaviorExpression Lifecycle<TLifecycle>() where TLifecycle : Lifecycle
        {
            config.Lifecycle = (Lifecycle)Activator.CreateInstance(typeof(TLifecycle));
            return this;
        }

        public ClassBehaviorExpression Lifecycle(Lifecycle lifecycle)
        {
            config.Lifecycle = lifecycle;
            return this;
        }

        public ClassBehaviorExpression Lifecycle(LifecycleAction lifecycle)
        {
            config.Lifecycle = new LambdaLifecycle(lifecycle);
            return this;
        }

        /// <summary>
        /// Randomizes the order of execution of a test class's contained test cases, using the
        /// given pseudo-random number generator.
        /// </summary>
        public ClassBehaviorExpression ShuffleCases(Random random)
        {
            config.OrderCases = cases => Shuffle(cases, random);
            return this;
        }

        /// <summary>
        /// Randomizes the order of execution of a test class's contained test cases.
        /// </summary>
        public ClassBehaviorExpression ShuffleCases()
        {
            return ShuffleCases(new Random());
        }

        /// <summary>
        /// Defines the order of execution of a test class's contained test cases.
        /// </summary>
        public ClassBehaviorExpression SortCases(Comparison<Case> comparison)
        {
            config.OrderCases = cases => Array.Sort(cases, comparison);
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

        class LambdaLifecycle : Lifecycle
        {
            readonly LifecycleAction execute;

            public LambdaLifecycle(LifecycleAction execute)
                => this.execute = execute;

            public void Execute(Type testClass, Action<CaseAction> runCases)
                => execute(testClass, runCases);
        }
    }
}