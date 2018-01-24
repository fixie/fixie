namespace Fixie.Conventions
{
    using System;

    public class CaseBehaviorExpression
    {
        readonly Configuration config;

        internal CaseBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Allows the specified predicate to determine whether a given test case should be skipped
        /// during execution. Skipped test cases are never executed, but are counted and identified.
        /// When a test is skipped, the behavior may include an optional explanation in the output.
        /// </summary>
        public CaseBehaviorExpression Skip<TSkipBehavior>() where TSkipBehavior : SkipBehavior
        {
            var behavior = (SkipBehavior)Activator.CreateInstance(typeof(TSkipBehavior));
            return Skip(behavior);
        }

        /// <summary>
        /// Allows the specified predicate to determine whether a given test case should be skipped
        /// during execution. Skipped test cases are never executed, but are counted and identified.
        /// When a test is skipped, the behavior may include an optional explanation in the output.
        /// </summary>
        public CaseBehaviorExpression Skip(SkipBehavior behavior)
        {
            return Skip(behavior.SkipCase, behavior.GetSkipReason);
        }

        /// <summary>
        /// Allows the specified predicate to determine whether a given test case should be skipped
        /// during execution. Skipped test cases are never executed, but are counted and identified.
        /// When a test is skipped, this overload will not include an explanation in the output.
        /// </summary>
        public CaseBehaviorExpression Skip(Func<Case, bool> skipCase)
        {
            return Skip(skipCase, @case => null);
        }

        /// <summary>
        /// Allows the specified predicate to determine whether a given test case should be skipped
        /// during execution. Skipped test cases are never executed, but are counted and identified.
        /// When a test is skipped, the specified reason generator will be invoked to include an
        /// explanation in the output.
        /// </summary>
        public CaseBehaviorExpression Skip(Func<Case, bool> skipCase, Func<Case, string> getSkipReason)
        {
            config.AddSkipBehavior(new LambdaSkipBehavior(skipCase, getSkipReason));
            return this;
        }

        class LambdaSkipBehavior : SkipBehavior
        {
            readonly Func<Case, bool> skipCase;
            readonly Func<Case, string> getSkipReason;

            public LambdaSkipBehavior(Func<Case, bool> skipCase, Func<Case, string> getSkipReason)
            {
                this.skipCase = skipCase;
                this.getSkipReason = getSkipReason;
            }

            public bool SkipCase(Case @case)
                => skipCase(@case);

            public string GetSkipReason(Case @case)
                => getSkipReason(@case);
        }
    }
}