using System;
using Fixie.Internal;

namespace Fixie.Conventions
{
    public class CaseBehaviorExpression
    {
        readonly Configuration config;

        internal CaseBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Wraps each test case with the specified behavior. The behavior may perform custom
        /// actions before and/or after each test case executes.
        /// </summary>
        public CaseBehaviorExpression Wrap<TCaseBehavior>() where TCaseBehavior : CaseBehavior
        {
            config.WrapCases(() => (CaseBehavior)Activator.CreateInstance(typeof(TCaseBehavior)));
            return this;
        }

        /// <summary>
        /// Wraps each test case with the specified behavior. The behavior may perform custom
        /// actions before and/or after each test case executes.
        /// </summary>
        public CaseBehaviorExpression Wrap(CaseBehavior behavior)
        {
            config.WrapCases(() => behavior);
            return this;
        }

        /// <summary>
        /// Wraps each test case with the specified behavior. The behavior may perform custom
        /// actions before and/or after each test case executes.
        /// </summary>
        public CaseBehaviorExpression Wrap(CaseBehaviorAction behavior)
        {
            config.WrapCases(() => new LambdaCaseBehavior(behavior));
            return this;
        }

        /// <summary>
        /// Allows the specified predicate to determine whether a given test case should be skipped
        /// during execution. Skipped test cases are never executed, but are counted and identified.
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
            config.SkipCase = skipCase;
            config.GetSkipReason = getSkipReason;
            return this;
        }

        class LambdaCaseBehavior : CaseBehavior
        {
            readonly CaseBehaviorAction execute;

            public LambdaCaseBehavior(CaseBehaviorAction execute)
            {
                this.execute = execute;
            }

            public void Execute(Case context, Action next)
            {
                execute(context, next);
            }
        }
    }
}