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
            public string FullName { get; set; }
            public string DisplayName { get; set; }
        }

        public abstract class TestResult
        {
            public string FullName { get; set; }
            public string DisplayName { get; set; }
            public TimeSpan Duration { get; set; }
            public string Output { get; set; }
        }

        public class SkipResult : TestResult
        {
            public string Reason { get; set; }
        }

        public class PassResult : TestResult
        {
        }

        public class FailResult : TestResult
        {
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