using System;
using Fixie.Internal;

namespace Fixie.Conventions
{
    public class ClassBehaviorExpression
    {
        readonly Configuration config;

        internal ClassBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Test classes will have one new instance created for each test case contained within
        /// it, allowing the constructor to perform test case setup. This is the default when no
        /// construction frequency is specified.
        /// </summary>
        public ClassBehaviorExpression CreateInstancePerCase()
        {
            config.ConstructionFrequency = ConstructionFrequency.PerCase;
            return this;
        }

        /// <summary>
        /// Test classes will have only one instance created. The single instance will be used
        /// for all of the test cases contained within it, allowing the constructor to perform
        /// test fixture setup.
        /// </summary>
        public ClassBehaviorExpression CreateInstancePerClass()
        {
            config.ConstructionFrequency = ConstructionFrequency.PerClass;
            return this;
        }

        /// <summary>
        /// Allows a custom test class factory to be used instead of calling the test class's
        /// default constructor.
        /// </summary>
        public ClassBehaviorExpression UsingFactory(Func<Type, object> customFactory)
        {
            config.TestClassFactory = customFactory;
            return this;
        }

        /// <summary>
        /// Wraps each test class with the specified behavior. The behavior may perform custom
        /// actions before and/or after each test class executes.
        /// </summary>
        public ClassBehaviorExpression Wrap<TClassBehavior>() where TClassBehavior : ClassBehavior
        {
            config.WrapClasses(() => (ClassBehavior)Activator.CreateInstance(typeof(TClassBehavior)));
            return this;
        }

        /// <summary>
        /// Wraps each test class with the specified behavior. The behavior may perform custom
        /// actions before and/or after each test class executes.
        /// </summary>
        public ClassBehaviorExpression Wrap(ClassBehavior behavior)
        {
            config.WrapClasses(() => behavior);
            return this;
        }

        /// <summary>
        /// Wraps each test class with the specified behavior. The behavior may perform custom
        /// actions before and/or after each test class executes.
        /// </summary>
        public ClassBehaviorExpression Wrap(ClassBehaviorAction behavior)
        {
            config.WrapClasses(() => new LambdaBehavior(behavior));
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

        class LambdaBehavior : ClassBehavior
        {
            readonly ClassBehaviorAction execute;

            public LambdaBehavior(ClassBehaviorAction execute)
            {
                this.execute = execute;
            }

            public void Execute(Class context, Action next)
            {
                execute(context, next);
            }
        }
    }
}