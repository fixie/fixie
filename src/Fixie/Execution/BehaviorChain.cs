namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;

    class BehaviorChain<TContext> where TContext : BehaviorContext
    {
        readonly List<Behavior<TContext>> behaviors;

        public BehaviorChain(params Behavior<TContext>[] behaviors)
            : this((IEnumerable<Behavior<TContext>>)behaviors) { }

        public BehaviorChain(IEnumerable<Behavior<TContext>> behaviors)
        {
            this.behaviors = new List<Behavior<TContext>>(behaviors);
        }

        public void Execute(TContext context)
        {
            if (behaviors.Count > 0)
                ExecuteNext(context, 0);
        }

        void ExecuteNext(TContext context, int index)
        {
            if (index > behaviors.Count - 1)
                return;

            try
            {
                behaviors[index].Execute(context, () => ExecuteNext(context, index + 1));
            }
            catch (Exception exception)
            {
                context.Fail(exception);
            }
        }
    }
}