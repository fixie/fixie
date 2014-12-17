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

        public FixtureBehaviorExpression Wrap<TFixtureBehavior>() where TFixtureBehavior : FixtureBehavior
        {
            config.WrapFixtures<TFixtureBehavior>();
            return this;
        }
    }
}