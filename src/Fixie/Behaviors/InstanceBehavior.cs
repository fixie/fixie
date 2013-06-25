using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public interface InstanceBehavior
    {
        void Execute(Fixture fixture, Convention convention);
    }
}