using System;

namespace Fixie.Internal
{
    public class SkipRule
    {
        readonly Func<Case, bool> skipCase;
        readonly Func<Case, string> getSkipReason;

        public SkipRule(Func<Case, bool> skipCase, Func<Case, string> getSkipReason)
        {
            this.skipCase = skipCase;
            this.getSkipReason = getSkipReason;
        }

        public bool AppliesTo(Case @case, out string reason)
        {
            if (SkipCase(@case))
            {
                reason = GetSkipReason(@case);
                return true;
            }

            reason = null;
            return false;
        }

        bool SkipCase(Case @case)
        {
            try
            {
                return skipCase(@case);
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown while attempting to run a custom case-skipping predicate. " +
                    "Check the inner exception for more details.", exception);
            }
        }

        string GetSkipReason(Case @case)
        {
            try
            {
                return getSkipReason(@case);
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown while attempting to get a custom case-skipped reason. " +
                    "Check the inner exception for more details.", exception);
            }
        }
    }
}