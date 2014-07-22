using System;

namespace Fixie.Execution
{
    public interface Behavior<in TContext> where TContext : BehaviorContext
    {
        void Execute(TContext context, Action next);
    }
}