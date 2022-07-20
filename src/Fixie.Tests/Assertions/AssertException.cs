namespace Fixie.Tests.Assertions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using static System.Environment;

    [Serializable]
    public class AssertException : Exception
    {

        public string? Expected { get; }
        public string? Actual { get; }
        public bool HasCompactRepresentations { get; }

        public AssertException(string? expected, string? actual)
        {
            Expected = expected;
            Actual = actual;
            HasCompactRepresentations = HasCompactRepresentation(expected) &&
                                        HasCompactRepresentation(actual);
        }

        public override string Message
        {
            get
            {
                var expected = Expected ?? "null";
                var actual = Actual ?? "null";

                if (HasCompactRepresentations)
                    return $"Expected: {expected}{NewLine}" +
                           $"Actual:   {actual}";

                return $"Expected:{NewLine}{expected}{NewLine}{NewLine}" +
                       $"Actual:{NewLine}{actual}";
            }
        }

        static bool HasCompactRepresentation(string? value)
        {
            const int compactLength = 50;

            if (value is null)
                return true;

            return value.Length <= compactLength && !value.Contains(NewLine);
        }

        public override string? StackTrace => FilterStackTrace(base.StackTrace);

        public static string FilterStackTraceAssemblyPrefix { get; set; } = typeof(AssertException).Namespace + ".";

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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected AssertException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            :base(serializationInfo, streamingContext)
        {
        }

        public AssertException() : base()
        {
        }

        public AssertException(string? message) : base(message)
        {
        }

        public AssertException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}