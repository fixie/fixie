using System;
using Fixie.Internal;

namespace Fixie.Conventions
{
    public class FixtureBehaviorExpression
    {
        readonly Configuration config;

        internal FixtureBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Wraps each test fixture (test class instance) with the specified behavior.
        /// 
        /// <para>
        /// The behavior may perform custom actions before, after, or instead of
        /// executing each test fixture. Invoke next() to proceed with normal execution.
        /// </para>
        /// </summary>
        public FixtureBehaviorExpression Wrap<TFixtureBehavior>() where TFixtureBehavior : FixtureBehavior
        {
            config.WrapFixtures(() => (FixtureBehavior)Activator.CreateInstance(typeof(TFixtureBehavior)));
            return this;
        }

        /// <summary>
        /// Wraps each test fixture (test class instance) with the specified behavior.
        /// 
        /// <para>
        /// The behavior may perform custom actions before, after, or instead of
        /// executing each test fixture. Invoke next() to proceed with normal execution.
        /// </para>
        /// </summary>
        public FixtureBehaviorExpression Wrap(FixtureBehavior behavior)
        {
            config.WrapFixtures(() => behavior);
            return this;
        }

        /// <summary>
        /// Wraps each test fixture (test class instance) with the specified behavior.
        /// 
        /// <para>
        /// The behavior may perform custom actions before, after, or instead of
        /// executing each test fixture. Invoke next() to proceed with normal execution.
        /// </para>
        /// </summary>
        public FixtureBehaviorExpression Wrap(FixtureBehaviorAction behavior)
        {
            config.WrapFixtures(() => new LambdaBehavior(behavior));
            return this;
        }

        class LambdaBehavior : FixtureBehavior
        {
            readonly FixtureBehaviorAction execute;

            public LambdaBehavior(FixtureBehaviorAction execute)
            {
                this.execute = execute;
            }

            public void Execute(Fixture context, Action next)
            {
                execute(context, next);
            }
        }
    }
}