using System;
using System.Collections.Generic;

namespace Should.Core.Exceptions
{
    public class AssertException : Exception
    {
        public static string FilterStackTraceAssemblyPrefix = "Should.";

        readonly string stackTrace;

        public AssertException() { }

        public AssertException(string userMessage)
            : base(userMessage)
        {
            this.UserMessage = userMessage;
        }

        protected AssertException(string userMessage, Exception innerException)
            : base(userMessage, innerException) { }

        protected AssertException(string userMessage, string stackTrace)
            : base(userMessage)
        {
            this.stackTrace = stackTrace;
        }

        public override string StackTrace
        {
            get { return FilterStackTrace(stackTrace ?? base.StackTrace); }
        }

        public string UserMessage { get; protected set; }

        protected static string FilterStackTrace(string stackTrace)
        {
            if (stackTrace == null)
                return null;

            List<string> results = new List<string>();

            foreach (string line in SplitLines(stackTrace))
            {
                string trimmedLine = line.TrimStart();
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