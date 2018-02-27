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
            config.AddSkipBehavior(new SkipBehavior(skipCase, getSkipReason));
            return this;
        }
    }
}