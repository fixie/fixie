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

        public ClassBehaviorExpression CreateInstancePerCase()
        {
            config.ConstructionFrequency = ConstructionFrequency.PerCase;
            return this;
        }

        public ClassBehaviorExpression CreateInstancePerClass()
        {
            config.ConstructionFrequency = ConstructionFrequency.PerClass;
            return this;
        }

        public ClassBehaviorExpression UsingFactory(Func<Type, object> customFactory)
        {
            config.TestClassFactory = customFactory;
            return this;
        }

        public ClassBehaviorExpression Wrap<TClassBehavior>() where TClassBehavior : ClassBehavior
        {
            config.WrapClasses<TClassBehavior>();
            return this;
        }

        public ClassBehaviorExpression ShuffleCases(Random random)
        {
            config.OrderCases = cases => Shuffle(cases, random);
            return this;
        }

        public ClassBehaviorExpression ShuffleCases()
        {
            return ShuffleCases(new Random());
        }

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
    }
}