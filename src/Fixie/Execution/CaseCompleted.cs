using System;

namespace Fixie.Execution
{
    public interface CaseCompleted
    {
        CaseStatus Status { get; }
        string Name  { get; }
        MethodGroup MethodGroup  { get; }
        string Output  { get; }
        TimeSpan Duration  { get; }
        CompoundException Exceptions { get; }
        string SkipReason { get; }
    }
}