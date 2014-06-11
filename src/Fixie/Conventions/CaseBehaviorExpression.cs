using System;
using System.Collections.Generic;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public class CaseBehaviorExpression
    {
        readonly List<Type> customCaseBehaviors = new List<Type>();

        public CaseBehaviorExpression()
        {
            SkipPredicate = @case => false;
            SkipReasonProvider = SkipReasonUnknown;
        }

        public BehaviorChain<CaseExecution> BuildBehaviorChain()
        {
            var chain = new BehaviorChain<CaseExecution>();

            foreach (var customBehavior in customCaseBehaviors)
                chain.Add((CaseBehavior)Activator.CreateInstance(customBehavior));

            chain.Add(new Invoke());

            return chain;
        }

        public Func<Case, bool> SkipPredicate { get; private set; }
        public Func<Case, string> SkipReasonProvider { get; private set; }

        public CaseBehaviorExpression Wrap<TCaseBehavior>() where TCaseBehavior : CaseBehavior
        {
            customCaseBehaviors.Insert(0, typeof(TCaseBehavior));
            return this;
        }

        public CaseBehaviorExpression Skip(Func<Case, bool> skipPredicate)
        {
            return Skip(skipPredicate, SkipReasonUnknown);
        }

        public CaseBehaviorExpression Skip(Func<Case, bool> skipPredicate, Func<Case, string> skipReasonProvider)
        {
            SkipPredicate = skipPredicate;
            SkipReasonProvider = skipReasonProvider;
            return this;
        }

        static string SkipReasonUnknown(Case @case)
        {
            return null;
        }
    }
}