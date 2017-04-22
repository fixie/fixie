namespace Fixie
{
    using System;

    public interface Behavior<in TContext> where TContext : BehaviorContext
    {
        /// <summary>
        /// Executes custom behavior against the given context, wrapping the next behavior in the chain.
        /// A custom behavior may perform actions before, after, or instead of the inner behavior being
        /// wrapped. Invoke next() to proceed with normal execution.
        /// </summary>
        void Execute(TContext context, Action next);
    }
}