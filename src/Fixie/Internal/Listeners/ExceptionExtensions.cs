namespace Fixie.Internal.Listeners
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

            if (lines.Length >= 2)
            {
                if (lines[^2].Contains(" Fixie.MethodInfoExtensions.Execute(MethodInfo method, Object instance, Object[] parameters)") &&
                    lines[^1].Contains(" Fixie.Case.Execute(Object instance)"))

                    return string.Join(NewLine, lines[..^2]);
            }

            return exception.StackTrace;
        }
    }
}