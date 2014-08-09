namespace Fixie.Conventions
{
    public class FixtureBehaviorExpression
    {
        readonly Configuration config;

        public FixtureBehaviorExpression(Configuration config)
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