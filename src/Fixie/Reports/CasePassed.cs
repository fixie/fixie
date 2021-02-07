namespace Fixie.Reports
{
    using System;
    using Internal;

    public class CasePassed : CaseCompleted
    {
        internal CasePassed(Case @case, TimeSpan duration, string output)
            : base(@case, duration, output)
        {
        }
    }
}