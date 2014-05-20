using System;
using System.Collections.Generic;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public delegate void CaseBehaviorAction(CaseExecution caseExecution, Action innerBehavior);
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

        public CaseBehaviorBuilder Wrap<TDisposable>() where TDisposable : IDisposable, new()
        {
            return Wrap((caseExecution, innerBehavior) =>
            {
                using (new TDisposable())
                    innerBehavior();
            });
        }

        public CaseBehaviorBuilder SetUp(CaseAction setUp)
        {
            return Wrap((caseExecution, innerBehavior) =>
            {
                setUp(caseExecution, caseExecution.Instance);
                innerBehavior();
            });
        }

        public CaseBehaviorBuilder SetUpTearDown(CaseAction setUp, CaseAction tearDown)
        {
            return Wrap((caseExecution, innerBehavior) =>
            {
                setUp(caseExecution, caseExecution.Instance);
                innerBehavior();
                tearDown(caseExecution, caseExecution.Instance);
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

            public void Execute(CaseExecution caseExecution)
            {
                try
                {
                    outer(caseExecution, () => inner.Execute(caseExecution));
                }
                catch (Exception exception)
                {
                    caseExecution.Fail(exception);
                }
            }
        }
    }
}