using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void CaseBehaviorAction(CaseExecution caseExecution, object instance, Action innerBehavior);
    public delegate void CaseAction(CaseExecution caseExecution, object instance);

    public class CaseBehaviorBuilder
    {
        public CaseBehaviorBuilder()
        {
            Behavior = new Invoke();
            SkipPredicate = @case => false;
        }

        public CaseBehavior Behavior { get; private set; }

        public Func<Case, bool> SkipPredicate { get; private set; }

        public CaseBehaviorBuilder Wrap(CaseBehaviorAction outer)
        {
            Behavior = new WrapBehavior(outer, Behavior);
            return this;
        }

        public CaseBehaviorBuilder SetUp(CaseAction setUp)
        {
            return Wrap((caseExecution, instance, innerBehavior) =>
            {
                setUp(caseExecution, instance);
                innerBehavior();
            });
        }

        public CaseBehaviorBuilder SetUpTearDown(CaseAction setUp, CaseAction tearDown)
        {
            return Wrap((caseExecution, instance, innerBehavior) =>
            {
                setUp(caseExecution, instance);
                innerBehavior();
                tearDown(caseExecution, instance);
            });
        }

        public CaseBehaviorBuilder Skip(Func<Case, bool> skipPredicate)
        {
            SkipPredicate = skipPredicate;
            return this;
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

            public void Execute(CaseExecution caseExecution, object instance)
            {
                try
                {
                    outer(caseExecution, instance, () => inner.Execute(caseExecution, instance));
                }
                catch (Exception exception)
                {
                    caseExecution.Fail(exception);
                }
            }
        }
    }
}