using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void CaseBehaviorAction(Case @case, object instance, CaseBehavior inner);
    public delegate void CaseAction(Case @case, object instance);

    public class CaseBehaviorBuilder
    {
        public CaseBehaviorBuilder()
        {
            Behavior = new Invoke();
        }

        public CaseBehavior Behavior { get; private set; }

        public CaseBehaviorBuilder Wrap(CaseBehaviorAction outer)
        {
            Behavior = new WrapBehavior(outer, Behavior);
            return this;
        }

        public CaseBehaviorBuilder SetUpTearDown(CaseAction setUp, CaseAction tearDown)
        {
            return Wrap((@case, instance, inner) =>
            {
                if (@case.Exceptions.Any())
                    return;

                setUp(@case, instance);

                if (@case.Exceptions.Any())
                    return;

                inner.Execute(@case, instance);
                tearDown(@case, instance);
            });
        }

        public CaseBehaviorBuilder SetUpTearDown(MethodFilter setUpMethods, MethodFilter tearDownMethods)
        {
            return SetUpTearDown((@case, instance) => InvokeAll(setUpMethods, @case, instance),
                                 (@case, instance) => InvokeAll(tearDownMethods, @case, instance));
        }

        static void InvokeAll(MethodFilter methodFilter, Case @case, object instance)
        {
            var invoke = new Invoke();
            foreach (var method in methodFilter.Filter(@case.Class))
                invoke.Execute(method, instance, @case.Exceptions);
        }

        class WrapBehavior : CaseBehavior
        {
            readonly CaseBehaviorAction outer;
            readonly CaseBehavior inner;

            public WrapBehavior(CaseBehaviorAction outer, CaseBehavior inner)
            {
                this.outer = outer;
                this.inner = inner;
            }

            public void Execute(Case @case, object instance)
            {
                try
                {
                    outer(@case, instance, inner);
                }
                catch (Exception exception)
                {
                    @case.Exceptions.Add(exception);
                }
            }
        }
    }
}