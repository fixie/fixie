using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public interface InstanceBehavior
    {
        void Execute(Fixture fixture, Case[] cases, Convention convention);
    }
}