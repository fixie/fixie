using System;

namespace Fixie.Execution
{
    public interface CaseCompleted
    {
        MethodGroup MethodGroup  { get; }
        string Name  { get; }
        CaseStatus Status { get; }
        TimeSpan Duration  { get; }
        string Output  { get; }
        CompoundException Exceptions { get; }
        string SkipReason { get; }
    }
}