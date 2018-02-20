namespace Fixie.Execution
{
    using System;
    using System.IO;

    public class CaseFailed : CaseCompleted
    {
        public CaseFailed(Case @case)
            : base(@case)
        {
            var exception = @case.Exception;

            Exception = exception;
            StackTrace = GetCompoundStackTrace(exception);
        }

        public Exception Exception { get; }
        public string StackTrace { get; }

        static string GetCompoundStackTrace(Exception exception)
        {
            using (var console = new StringWriter())
            {
                var ex = exception;

                console.Write(ex.StackTrace);

                var walk = ex;
                while (walk.InnerException != null)
                {
                    walk = walk.InnerException;
                    console.WriteLine();
                    console.WriteLine();
                    console.WriteLine($"------- Inner Exception: {walk.GetType().FullName} -------");
                    console.WriteLine(walk.Message);
                    console.Write(walk.StackTrace);
                }

                return console.ToString();
            }
        }
    }
}