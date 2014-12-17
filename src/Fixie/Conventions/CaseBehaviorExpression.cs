using System;

namespace Fixie.Conventions
{
    public class CaseBehaviorExpression
    {
        readonly Configuration config;

        internal CaseBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        public CaseBehaviorExpression Wrap<TCaseBehavior>() where TCaseBehavior : CaseBehavior
        {
            config.WrapCases<TCaseBehavior>();
            return this;
        }

        public CaseBehaviorExpression Skip(Func<Case, bool> skipCase)
        {
            return Skip(skipCase, @case => null);
        }

        public CaseBehaviorExpression Skip(Func<Case, bool> skipCase, Func<Case, string> getSkipReason)
        {
            config.SkipCase = skipCase;
            config.GetSkipReason = getSkipReason;
            return this;
        }
    }
}