namespace Fixie.Conventions
{
    public class InstanceBehaviorExpression
    {
        readonly Configuration config;

        public InstanceBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        public InstanceBehaviorExpression Wrap<TInstanceBehavior>() where TInstanceBehavior : FixtureBehavior
        {
            config.WrapInstances<TInstanceBehavior>();
            return this;
        }
    }
}