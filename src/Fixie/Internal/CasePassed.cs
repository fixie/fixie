namespace Fixie.Internal
{
    using System;

    public class CasePassed : CaseCompleted
    {
        public CasePassed(Case @case, TimeSpan duration)
            : base(@case, duration)
        {
        }
    }
}