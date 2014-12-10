using System;
using Fixie.Discovery;

namespace Fixie.Results
{
    public interface CaseResult
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