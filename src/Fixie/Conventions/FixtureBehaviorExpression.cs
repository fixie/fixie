namespace Fixie.Conventions
{
    public class FixtureBehaviorExpression
    {
        readonly Configuration config;

        public FixtureBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        public FixtureBehaviorExpression Wrap<TInstanceBehavior>() where TInstanceBehavior : FixtureBehavior
        {
            config.WrapInstances<TInstanceBehavior>();
            return this;
        }
    }
}