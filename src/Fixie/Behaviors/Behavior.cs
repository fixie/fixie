using System;

namespace Fixie.Behaviors
{
    public interface Behavior<in TContext> where TContext : BehaviorContext
    {
        void Execute(TContext context, Action next);
    }
}