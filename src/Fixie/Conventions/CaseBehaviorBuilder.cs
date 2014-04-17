using System;
using System.Collections.Generic;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void CaseBehaviorAction(CaseExecution caseExecution, object instance, Action innerBehavior);
    public delegate void CaseAction(CaseExecution caseExecution, object instance);

    public class CaseBehaviorBuilder
    {
        readonly List<CaseBehaviorAction> customBehaviors = new List<CaseBehaviorAction>();

        public CaseBehaviorBuilder()
        {
            SkipPredicate = @case => false;
            SkipReasonProvider = SkipReasonUnknown;
        }

        public CaseBehavior BuildBehavior()
        {
            CaseBehavior behavior = new Invoke();

            foreach (var customBehavior in customBehaviors)
                behavior = new WrapBehavior(customBehavior, behavior);

            return behavior;
        }

        public Func<Case, bool> SkipPredicate { get; private set; }
        public Func<Case, string> SkipReasonProvider { get; private set; }

        public CaseBehaviorBuilder Wrap(CaseBehaviorAction outer)
        {
            customBehaviors.Add(outer);
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
            return Skip(skipPredicate, SkipReasonUnknown);
        }

        public CaseBehaviorBuilder Skip(Func<Case, bool> skipPredicate, Func<Case, string> skipReasonProvider)
        {
            SkipPredicate = skipPredicate;
            SkipReasonProvider = skipReasonProvider;
            return this;
        }

        static string SkipReasonUnknown(Case @case)
        {
            return null;
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