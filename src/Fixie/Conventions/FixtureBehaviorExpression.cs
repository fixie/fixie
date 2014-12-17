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
        /// Wraps each test fixture (test class instance) with the specified behavior type. The
        /// behavior may perform custom actions before and/or after each test fixture executes.
        /// </summary>
        public FixtureBehaviorExpression Wrap<TFixtureBehavior>() where TFixtureBehavior : FixtureBehavior
        {
            config.WrapFixtures<TFixtureBehavior>();
            return this;
        }
    }
}