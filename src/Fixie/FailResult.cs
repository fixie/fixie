using System;
using System.Collections.Generic;
using System.Linq;
using Fixie.Results;

namespace Fixie
{
    public class FailResult
    {
        public FailResult(CaseExecution execution)
        {
            Case = execution.Case;
            Output = execution.Output;
            Duration = execution.Duration;
            Exceptions = execution.Exceptions.Select(x => new ExceptionInfo(x)).ToArray();
        }

        public Case Case { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
        public IReadOnlyList<ExceptionInfo> Exceptions { get; private set; }
    }
}