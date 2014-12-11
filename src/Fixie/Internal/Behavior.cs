using System;

namespace Fixie.Internal
{
    public interface Behavior<in TContext> where TContext : BehaviorContext
    {
        void Execute(TContext context, Action next);
    }
}