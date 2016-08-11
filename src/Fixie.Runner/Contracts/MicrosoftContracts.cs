namespace Fixie.Runner.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Newtonsoft.Json.Linq;

    // The Microsoft.Extensions.Testing.Abstractions NuGet package defines several contract types used as JSON payloads
    // as part of the "dotnet test protocol". Referencing that package, however, brings in a suspicious number of transient
    // dependencies that Fixie has no actual need for. Because Fixie needs only to create JSON messages of the right shape,
    // we redundantly define the contract types here rather than take on many excessive dependencies.

    public class Message
    {
        public string MessageType { get; set; }
        public JToken Payload { get; set; }
    }

    public class Test
    {
        public Test()
        {
            Properties = new Dictionary<string, object>(StringComparer.Ordinal);
        }

        public string CodeFilePath { get; set; }
        public string DisplayName { get; set; }
        public string FullyQualifiedName { get; set; }
        public Guid? Id { get; set; }
        public int? LineNumber { get; set; }
        public IDictionary<string, object> Properties { get; private set; }
    }

    public class TestResult
    {
        public TestResult(Test test)
        {
            Test = test;
            Messages = new Collection<string>();
        }

        public Test Test { get; private set; }
        public TestOutcome Outcome { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackTrace { get; set; }
        public string DisplayName { get; set; }
        public Collection<string> Messages { get; private set; }
        public string ComputerName { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }

    public enum TestOutcome
    {
        None, Passed, Failed, Skipped, NotFound,
    }

    public class RunTestsMessage
    {
        public List<string> Tests { get; set; }
    }
}