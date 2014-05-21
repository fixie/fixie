using System;

namespace Fixie.Behaviors
{
    public interface Behavior<in TContext>
    {
        void Execute(TContext context, Action next);
    }
}