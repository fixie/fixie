﻿using Fixie.Execution;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VisualStudioListener : Listener
    {
        readonly IExecutionSink log;

        public VisualStudioListener(IExecutionSink log)
        {
            this.log = log;
        }

        public void Handle(AssemblyStarted message) { }

        public void Handle(SkipResult result)
        {
            log.RecordResult(new CaseResult(result));
        }

        public void Handle(PassResult result)
        {
            log.RecordResult(new CaseResult(result));
        }

        public void Handle(FailResult result)
        {
            log.RecordResult(new CaseResult(result));
        }

        public void Handle(AssemblyCompleted message)
        {
            log.SendMessage(message.Result.Summary);
        }
    }
}