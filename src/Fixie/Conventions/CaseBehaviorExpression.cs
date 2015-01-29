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

        /// <summary>
        /// Allows customizing the name of a given case.
        /// </summary>
        public CaseBehaviorExpression Name(Func<Case, string> getCaseName)
        {
            config.GetCaseName = getCaseName;
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
            {
                execute(context, next);
            }
        }
    }
}