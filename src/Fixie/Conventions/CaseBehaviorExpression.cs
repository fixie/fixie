namespace Fixie.Conventions
{
    using System;
    using Internal;

    public class CaseBehaviorExpression
    {
        readonly Configuration config;

        internal CaseBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Wraps each test case with the specified behavior.
        /// 
        /// <para>
        /// The behavior may perform custom actions before, after, or instead of
        /// executing each test case. Invoke next() to proceed with normal execution.
        /// </para>
        /// </summary>
        public CaseBehaviorExpression Wrap<TCaseBehavior>() where TCaseBehavior : CaseBehavior
        {
            config.WrapCases(() => (CaseBehavior)Activator.CreateInstance(typeof(TCaseBehavior)));
            return this;
        }

        /// <summary>
        /// Wraps each test case with the specified behavior.
        /// 
        /// <para>
        /// The behavior may perform custom actions before, after, or instead of
        /// executing each test case. Invoke next() to proceed with normal execution.
        /// </para>
        /// </summary>
        public CaseBehaviorExpression Wrap(CaseBehavior behavior)
        {
            config.WrapCases(() => behavior);
            return this;
        }

        /// <summary>
        /// Wraps each test case with the specified behavior.
        /// 
        /// <para>
        /// The behavior may perform custom actions before, after, or instead of
        /// executing each test case. Invoke next() to proceed with normal execution.
        /// </para>
        /// </summary>
        public CaseBehaviorExpression Wrap(CaseBehaviorAction behavior)
        {
            config.WrapCases(() => new LambdaBehavior(behavior));
            return this;
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

        class LambdaBehavior : CaseBehavior
        {
            readonly CaseBehaviorAction execute;

            public LambdaBehavior(CaseBehaviorAction execute)
            {
                this.execute = execute;
            }

            public void Execute(Case context, Action next)
                => execute(context, next);
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

            public override bool SkipCase(Case @case)
                => skipCase(@case);

            public override string GetSkipReason(Case @case)
                => getSkipReason(@case);
        }
    }
}