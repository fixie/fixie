using System;
using System.Collections.Generic;

namespace Should.Core.Exceptions
{
    public class AssertException : Exception
    {
        public static string FilterStackTraceAssemblyPrefix = "Should.";

        public AssertException(string message)
            : base(message)
        {
        }

        public override string StackTrace => FilterStackTrace(base.StackTrace);

        static string FilterStackTrace(string stackTrace)
        {
            if (stackTrace == null)
                return null;

            var results = new List<string>();

            foreach (var line in SplitLines(stackTrace))
            {
                var trimmedLine = line.TrimStart();
                if (!trimmedLine.StartsWith( "at " + FilterStackTraceAssemblyPrefix) )
                    results.Add(line);
            }

            return string.Join(Environment.NewLine, results.ToArray());
        }

        // Our own custom String.Split because Silverlight/CoreCLR doesn't support the version we were using
        static IEnumerable<string> SplitLines(string input)
        {
            while (true)
            {
                int idx = input.IndexOf(Environment.NewLine);

                if (idx < 0)
                {
                    yield return input;
                    break;
                }

                yield return input.Substring(0, idx);
                input = input.Substring(idx + Environment.NewLine.Length);
            }
        }
    }
}