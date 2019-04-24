namespace Fixie.Assertions
{
    using System;
    using System.Collections.Generic;

    public class AssertException : Exception
    {
        public static string FilterStackTraceAssemblyPrefix = "Fixie.Assertions.";

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

            foreach (var line in Lines(stackTrace))
            {
                var trimmedLine = line.TrimStart();
                if (!trimmedLine.StartsWith("at " + FilterStackTraceAssemblyPrefix))
                    results.Add(line);
            }

            return string.Join(Environment.NewLine, results.ToArray());
        }

        static string[] Lines(string input)
        {
            return input.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
        }
    }
}