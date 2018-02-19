namespace Fixie.Execution
{
    using System;
    using System.IO;

    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case, AssertionLibraryFilter filter)
            : base(@case)
        {
            var exception = @case.Exception;

            Exception = exception;
            FailedAssertion = filter.IsFailedAssertion(exception);
            StackTrace = GetCompoundStackTrace(exception, filter);
        }

        public Exception Exception { get; }
        public bool FailedAssertion { get; }
        public string StackTrace { get; }

        public string TypedStackTrace()
        {
            if (FailedAssertion)
                return StackTrace;

            return Exception.GetType().FullName + Environment.NewLine + StackTrace;
        }

        static string GetCompoundStackTrace(Exception exception, AssertionLibraryFilter filter)
        {
            using (var console = new StringWriter())
            {
                var ex = exception;

                console.Write(filter.FilterStackTrace(ex));

                var walk = ex;
                while (walk.InnerException != null)
                {
                    walk = walk.InnerException;
                    console.WriteLine();
                    console.WriteLine();
                    console.WriteLine($"------- Inner Exception: {walk.GetType().FullName} -------");
                    console.WriteLine(walk.Message);
                    console.Write(filter.FilterStackTrace(walk));
                }

                return console.ToString();
            }
        }
    }
}