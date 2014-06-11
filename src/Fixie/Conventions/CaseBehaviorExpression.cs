using System;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public class CaseBehaviorExpression
    {
        readonly ConfigModel config;

        public CaseBehaviorExpression(ConfigModel config)
        {
            this.config = config;
            SkipPredicate = @case => false;
            SkipReasonProvider = SkipReasonUnknown;
        }

        public Func<Case, bool> SkipPredicate { get; private set; }
        public Func<Case, string> SkipReasonProvider { get; private set; }

        public CaseBehaviorExpression Wrap<TCaseBehavior>() where TCaseBehavior : CaseBehavior
        {
            config.WrapCases<TCaseBehavior>();
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