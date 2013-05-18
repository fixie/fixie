using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public interface TypeBehavior
    {
        void Execute(Type fixtureClass, Convention convention, Listener listener);
    }
}