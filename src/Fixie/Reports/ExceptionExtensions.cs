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
            // Test method invocation usually produces clean stack traces.
            // Async test stack traces, however, include test runner implementation
            // details which distract the user in their diagnosing of a test failure.
            // Similar to the way that assertion libraries simplify their assertion
            // exception stack traces to omit implementation details of the assertion
            // library, we omit these test method invocation implementation details as
            // well.

            if (exception.StackTrace == null)
                return null;

            var lines = exception.StackTrace.Split(NewLine);

            if (lines.Length >= 3)
            {
                if (lines[^3].Contains(" Fixie.Internal.MethodInfoExtensions.RunTestMethodAsync(MethodInfo method, Object instance, Object[] parameters)") &&
                    lines[^2].Contains(" Fixie.Internal.Case.RunAsync(Object instance)") &&
                    lines[^1].Contains(" Fixie.Test.RunCoreAsync(Object instance, Object[] parameters)"))

                    return string.Join(NewLine, lines[..^3]);
            }

            return exception.StackTrace;
        }
    }
}