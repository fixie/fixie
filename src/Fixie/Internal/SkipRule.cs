using System;

namespace Fixie.Internal
{
    public class SkipRule
    {
        public static readonly SkipRule DoNotSkip = new SkipRule(@case => false, @case => null);

        public SkipRule(Func<Case, bool> skipCase, Func<Case, string> getSkipReason)
        {
            SkipCase = skipCase;
            GetSkipReason = getSkipReason;
        }

        public Func<Case, bool> SkipCase { get; private set; }
        public Func<Case, string> GetSkipReason { get; private set; }
    }
}