namespace Fixie.Execution
{
    using System;

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