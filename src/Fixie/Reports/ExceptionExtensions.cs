namespace Fixie.Reports
{
    using System;
    using System.Text;
    using static System.Environment;

    static class ExceptionExtensions
    {

        public static string LiterateStackTrace(this Exception exception)
        {
            var console = new StringBuilder();

            console.Append(StackTraceOmittingInternalTestMethodFrames(exception));

            var walk = exception;
            while (walk.InnerException != null)
            {
                walk = walk.InnerException;
                console.AppendLine();
                console.AppendLine();
                console.AppendLine($"------- Inner Exception: {walk.GetType().FullName} -------");
                console.AppendLine(walk.Message);
                console.Append(walk.StackTrace);
            }

            return console.ToString();
        }

        static string? StackTraceOmittingInternalTestMethodFrames(Exception exception)
        {
            // Test failure stack traces can include test runner implementation
            // details which distract the user in their diagnosing of a test failure.
            //
            // Similar to the way that assertion libraries simplify their assertion
            // exception stack traces to omit implementation details of the assertion
            // library, we omit these test method invocation implementation details as
            // well.
            //
            // When in a code path where the test runner catches, reports, and stops
            // propagation of rethrown reflection exceptions, we have an opportunity
            // to trim that fully-managed rethrow noise from the end of the stack
            // trace. In any other scenario, we provide the full stack trace as it
            // will contain end user code relevant to diagnosing the exception.

            if (exception.StackTrace == null)
                return null;

            var lines = exception.StackTrace.Split(NewLine);

            if (lines.Length >= 2)
            {
                const string synchronousRethrowMarker = "--- End of stack trace from previous location where exception was thrown ---";
                const string runTestMethodAsync = " Fixie.MethodInfoExtensions.RunTestMethodAsync(MethodInfo method, Object instance, Object[] parameters)";
                const string constructTestClass = " Fixie.Test.Construct(Type testClass)";
                const string runCoreAsync = " Fixie.Test.RunCoreAsync(Object instance, Object[] parameters)";

                if (lines[^1].Contains(runCoreAsync))
                {
                    if (lines[^2].Contains(runTestMethodAsync) ||
                        lines[^2].Contains(constructTestClass))
                    {
                        var linesToRemove =
                            lines.Length >= 3 && lines[^3].Contains(synchronousRethrowMarker)
                                ? 3
                                : 2;

                        return string.Join(NewLine, lines[..^linesToRemove]);
                    }
                }
            }

            return exception.StackTrace;
        }
    }
}