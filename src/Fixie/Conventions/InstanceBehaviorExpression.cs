using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public class InstanceBehaviorExpression
    {
        readonly ConfigModel config;

        public InstanceBehaviorExpression(ConfigModel config)
        {
            this.config = config;
        }

        public InstanceBehaviorExpression Wrap<TInstanceBehavior>() where TInstanceBehavior : InstanceBehavior
        {
            config.WrapInstances<TInstanceBehavior>();
            return this;
        }
    }
}