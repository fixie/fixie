using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public interface InstanceBehavior
    {
        void Execute(Type fixtureClass, object instance, Case[] cases, Convention convention);
    }
}