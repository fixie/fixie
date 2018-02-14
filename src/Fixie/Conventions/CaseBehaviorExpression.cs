namespace Fixie.Conventions
{
    using System;
    using System.Reflection;

    public class CaseBehaviorExpression
    {
        readonly Configuration config;

        internal CaseBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Allows the specified predicate to determine whether a given test method should be skipped
        /// during execution. Skipped test methods are never executed, but are counted and identified.
        /// When a test method is skipped, the behavior may include an optional explanation in the output.
        /// </summary>
        public CaseBehaviorExpression Skip<TSkipBehavior>() where TSkipBehavior : SkipBehavior
        {
            var behavior = (SkipBehavior)Activator.CreateInstance(typeof(TSkipBehavior));
            return Skip(behavior);
        }

        /// <summary>
        /// Allows the specified predicate to determine whether a given test method should be skipped
        /// during execution. Skipped test methods are never executed, but are counted and identified.
        /// When a test method is skipped, the behavior may include an optional explanation in the output.
        /// </summary>
        public CaseBehaviorExpression Skip(SkipBehavior behavior)
        {
            return Skip(behavior.SkipMethod, behavior.GetSkipReason);
        }

        /// <summary>
        /// Allows the specified predicate to determine whether a given test method should be skipped
        /// during execution. Skipped test methods are never executed, but are counted and identified.
        /// When a test method is skipped, this overload will not include an explanation in the output.
        /// </summary>
        public CaseBehaviorExpression Skip(Func<MethodInfo, bool> skipCase)
        {
            return Skip(skipCase, @case => null);
        }

        /// <summary>
        /// Allows the specified predicate to determine whether a given test method should be skipped
        /// during execution. Skipped test methods are never executed, but are counted and identified.
        /// When a test method is skipped, the specified reason generator will be invoked to include an
        /// explanation in the output.
        /// </summary>
        public CaseBehaviorExpression Skip(Func<MethodInfo, bool> skipCase, Func<MethodInfo, string> getSkipReason)
        {
            config.AddSkipBehavior(new LambdaSkipBehavior(skipCase, getSkipReason));
            return this;
        }

        class LambdaSkipBehavior : SkipBehavior
        {
            readonly Func<MethodInfo, bool> skipMethod;
            readonly Func<MethodInfo, string> getSkipReason;

            public LambdaSkipBehavior(Func<MethodInfo, bool> skipMethod, Func<MethodInfo, string> getSkipReason)
            {
                this.skipMethod = skipMethod;
                this.getSkipReason = getSkipReason;
            }

            public bool SkipMethod(MethodInfo testMethod)
                => skipMethod(testMethod);

            public string GetSkipReason(MethodInfo testMethod)
                => getSkipReason(testMethod);
        }
    }
}