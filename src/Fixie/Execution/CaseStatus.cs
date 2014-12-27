using System;

namespace Fixie.Execution
{
    [Serializable]
    public enum CaseStatus
    {
        Passed,
        Failed,
        Skipped,
        Inconclusive
    }
}