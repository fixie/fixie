namespace Fixie.Execution
{
    using System;
    using System.IO;

    public class CompoundException
    {
        public CompoundException(Exception exception, AssertionLibraryFilter filter)
        {
            var primary = exception;
            FailedAssertion = filter.IsFailedAssertion(primary);
            StackTrace = GetCompoundStackTrace(exception, filter);
        }

        public bool FailedAssertion { get; }
        public string StackTrace { get; }

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