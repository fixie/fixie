﻿using Fixie.Execution;
using TestDriven.Framework;

namespace Fixie.TestDriven
{
    public class TestDrivenListener :
        IHandler<SkipResult>,
        IHandler<PassResult>,
        IHandler<FailResult>
    {
        readonly ITestListener tdnet;

        public TestDrivenListener(ITestListener tdnet)
        {
            this.tdnet = tdnet;
        }

        public void Handle(SkipResult result)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = result.Name,
                State = TestState.Ignored,
                Message = result.SkipReason
            });
        }

        public void Handle(PassResult result)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = result.Name,
                State = TestState.Passed
            });
        }

        public void Handle(FailResult result)
        {
            tdnet.TestFinished(new TestResult
            {
                Name = result.Name,
                State = TestState.Failed,
                Message = result.Exceptions.PrimaryException.DisplayName,
                StackTrace = result.Exceptions.CompoundStackTrace,
            });
        }
    }
}