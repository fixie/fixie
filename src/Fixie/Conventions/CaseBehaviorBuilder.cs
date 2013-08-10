using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void CaseBehaviorAction(Case @case, object instance, Action innerBehavior);
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
            return Wrap((@case, instance, innerBehavior) =>
            {
                setUp(@case, instance);
                innerBehavior();
                tearDown(@case, instance);
            });
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
                    outer(@case, instance, () => inner.Execute(@case, instance));
                }
                catch (PreservedException preservedException)
                {
                    @case.Fail(preservedException.OriginalException);
                }
                catch (Exception exception)
                {
                    @case.Fail(exception);
                }
            }
        }
    }
}