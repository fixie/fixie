namespace Fixie.Tests.Assertions
{
    using System;
    using System.Collections.Generic;
    using static System.Environment;

    public class AssertException : Exception
    {
        public static string FilterStackTraceAssemblyPrefix = typeof(AssertException).Namespace + ".";

        public AssertException(string message)
            : base(message)
        {
        }

        public override string? StackTrace => FilterStackTrace(base.StackTrace);

        static string? FilterStackTrace(string? stackTrace)
        {
            if (stackTrace == null)
                return null;

            var results = new List<string>();

            foreach (var line in Lines(stackTrace))
            {
                var trimmedLine = line.TrimStart();
                if (!trimmedLine.StartsWith("at " + FilterStackTraceAssemblyPrefix))
                    results.Add(line);
            }

            return string.Join(NewLine, results.ToArray());
        }

        static string[] Lines(string input)
        {
            return input.Split(new[] {NewLine}, StringSplitOptions.None);
        }
    }
}