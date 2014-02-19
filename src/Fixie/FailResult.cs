using System;
using System.Collections.Generic;
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
            Exceptions = execution.Exceptions;

            ExceptionSummary = new ExceptionInfo(Exceptions);
        }

        public Case Case { get; private set; }
        public string Output { get; private set; }
        public TimeSpan Duration { get; private set; }
        public IReadOnlyList<Exception> Exceptions { get; private set; }
        public ExceptionInfo ExceptionSummary { get; private set; }
    }
}