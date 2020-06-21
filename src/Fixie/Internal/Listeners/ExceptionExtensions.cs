namespace Fixie.Internal.Listeners
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Text;

    static class ExceptionExtensions
    {
        static readonly MethodInfo CaseExecuteMethod =
            typeof(Case).GetMethod("Execute")
            ?? throw new Exception("Could not find expected method Case.Execute(...).");

        static readonly MethodInfo ExceptionRethrowMethod =
            typeof(ExceptionDispatchInfo).GetMethod("Throw", new Type[] { })
            ?? throw new Exception("Could not find expected method ExceptionDispatchInfo.Throw().");

        public static string LiterateStackTrace(this Exception exception)
        {
            var console = new StringBuilder();

            var ex = exception;

            console.Append(StackTraceOmittingInternalRethrow(ex));

            var walk = ex;
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

        public static string? StackTraceOmittingInternalRethrow(Exception exception)
        {
            // If the stack trace ends with a well-known internal segment caused by
            // ExceptionDispatchInfo.Throw(), attempt to omit that section.
            //
            // Example: Synchronous test failures end with the following lines to omit:
            //
            //  --- End of stack trace from previous location where exception was thrown ---
            //    at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
            //    at Fixie.MethodInfoExtensions.Execute(MethodInfo method, Object instance, Object[] parameters)
            //    at Fixie.Case.Execute(Object instance)
            //
            // Example: Asynchronous test failures end with the following lines to omit:
            //
            // --- End of stack trace from previous location where exception was thrown ---
            //    at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
            //    at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
            //    at System.Runtime.CompilerServices.TaskAwaiter.GetResult()
            //    at Fixie.MethodInfoExtensions.Execute(MethodInfo method, Object instance, Object[] parameters)
            //    at Fixie.Case.Execute(Object instance)
            //
            // This assumes that stack trace lines use the "---" formatting of the rethrow hint
            // across cultures, and that the stack frame lines do *not* contain "---" across cultures.
            //
            // Although the stack frames do still include ExceptionDispatchInfo.Throw() for completeness,
            // the corresponding line is omitted from the stack trace text.
            //
            // When in doubt, return the original stack trace.

            if (exception.StackTrace == null)
                return null;

            var stackTrace = new StackTrace(exception);
            var frames = stackTrace.GetFrames();

            if (frames == null || frames.Length == 0)
                return exception.StackTrace;

            var lastFrame = frames.Last();

            if (lastFrame == null || lastFrame.GetMethod() != CaseExecuteMethod)
                return exception.StackTrace;

            if (TryCountFramesToRemove(frames, out var numberOfTrailingStackFramesToRemove))
            {
                //Verify that the last numberOfTrailingStackFramesToRemove stack trace lines
                //do NOT start or end with "---", and that once removed, the last line DOES.

                var lines = exception.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

                if (lines.Count < numberOfTrailingStackFramesToRemove + 1)
                {
                    //Unexpected. Fail safe.
                    return exception.StackTrace;
                }

                for (int i = 1; i <= numberOfTrailingStackFramesToRemove; i++)
                {
                    var lastLine = lines.Last();

                    if (lastLine.Contains("---"))
                    {
                        // Expected an " at ..." line. Fail safe.
                        return exception.StackTrace;
                    }

                    lines.RemoveAt(lines.Count - 1);
                }

                var rethrowMarker = lines.Last().Trim();

                if (!rethrowMarker.StartsWith("---"))
                {
                    // Expected an "--- ... ---" rethrow marker line. Fail safe.
                    return exception.StackTrace;
                }

                lines.RemoveAt(lines.Count - 1);

                return string.Join(Environment.NewLine, lines);
            }

            return exception.StackTrace;
        }

        static bool TryCountFramesToRemove(StackFrame?[] frames, out int numberOfTrailingStackFramesToRemove)
        {
            numberOfTrailingStackFramesToRemove = 0;

            for (int i = frames.Length - 1; i >= 0; i--)
            {
                var frame = frames[i];

                if (frame != null && frame.GetMethod() == ExceptionRethrowMethod)
                    return true;

                numberOfTrailingStackFramesToRemove++;
            }

            return false;
        }
    }
}