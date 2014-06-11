using Fixie.Behaviors;
using Fixie.Conventions;

namespace Fixie.DSL
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