namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Execution;
    using Wrappers;

    public class VisualStudioExecutionListener :
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly TestExecutionRecorder log;
        readonly string assemblyPath;

        public VisualStudioExecutionListener(TestExecutionRecorder log, string assemblyPath)
        {
            this.log = log;
            this.assemblyPath = assemblyPath;
        }

        public void Handle(CaseSkipped message)
        {
            Log(message, x =>
            {
                x.ErrorMessage = message.Reason;
            });
        }

        public void Handle(CasePassed message)
        {
            Log(message);
        }

        public void Handle(CaseFailed message)
        {
            var exception = message.Exception;

            Log(message, x =>
            {
                x.ErrorMessage = exception.Message;
                x.ErrorStackTrace = TypedStackTrace(exception);
            });
        }

        static string TypedStackTrace(CompoundException exception)
        {
            if (exception.FailedAssertion)
                return exception.StackTrace;

            return exception.Type + Environment.NewLine + exception.StackTrace;
        }

        void Log(CaseCompleted message, Action<TestResultModel> customize = null)
        {
            var testResult = new TestResultModel
            {
                TestCase = new TestCaseModel
                {
                    MethodGroup = message.MethodGroup.FullName,
                    AssemblyPath = assemblyPath
                },
                Name = message.Name,
                Status = message.Status.ToString(),
                Duration = message.Duration,
                Output = message.Output
            };

            customize?.Invoke(testResult);

            log.RecordResult(testResult);
        }
    }
}