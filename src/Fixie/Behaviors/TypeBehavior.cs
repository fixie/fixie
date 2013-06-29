using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public interface TypeBehavior
    {
        void Execute(Type testClass, Convention convention, Case[] cases);
    }
}