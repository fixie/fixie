namespace Fixie.Execution.Listeners
{
    using System;

    public static class PipeMessage
    {
        public class DiscoverMethods { }

        public class RunAssembly { }

        public class RunMethods
        {
            public string[] Methods { get; set; }
        }

        public class Test
        {
            public string FullyQualifiedName { get; set; }
            public string DisplayName { get; set; }
        }

        public class TestResult
        {
            public string FullyQualifiedName { get; set; }
            public string DisplayName { get; set; }
            public string Outcome { get; set; }
            public TimeSpan Duration { get; set; }
            public string Output { get; set; }
            public string ErrorMessage { get; set; }
            public string ErrorStackTrace { get; set; }
        }

        public class Exception
        {
            public string Details { get; set; }
        }

        public class Completed { }
    }
}