﻿namespace Fixie.Reports
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using static System.Environment;

    class TeamCityReport :
        Handler<AssemblyStarted>,
        Handler<TestSkipped>,
        Handler<TestPassed>,
        Handler<TestFailed>,
        Handler<AssemblyCompleted>
    {
        readonly TextWriter console;

        internal static TeamCityReport? Create(TextWriter console)
        {
            if (GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null)
                return new TeamCityReport(console);

            return null;
        }

        public TeamCityReport(TextWriter console)
            => this.console = console;

        public void Handle(AssemblyStarted message)
        {
            Message("testSuiteStarted name='{0}'", message.Assembly.GetName().Name);
        }

        public void Handle(TestSkipped message)
        {
            TestStarted(message);
            Output(message);
            Message("testIgnored name='{0}' message='{1}'", message.Name, message.Reason);
            TestFinished(message);
        }

        public void Handle(TestPassed message)
        {
            TestStarted(message);
            Output(message);
            TestFinished(message);
        }

        public void Handle(TestFailed message)
        {
            var details =
                message.Reason.GetType().FullName +
                NewLine +
                message.Reason.LiterateStackTrace();

            TestStarted(message);
            Output(message);
            Message("testFailed name='{0}' message='{1}' details='{2}'", message.Name, message.Reason.Message, details);
            TestFinished(message);
        }

        public void Handle(AssemblyCompleted message)
        {
            Message("testSuiteFinished name='{0}'", message.Assembly.GetName().Name);
        }

        void TestStarted(TestCompleted message)
        {
            Message("testStarted name='{0}'", message.Name);
        }

        void TestFinished(TestCompleted message)
        {
            Message("testFinished name='{0}' duration='{1}'", message.Name, $"{message.Duration.TotalMilliseconds:0}");
        }

        void Message(string format, params string?[] args)
        {
            var encodedArgs = args.Select(Encode).Cast<object>().ToArray();
            console.WriteLine("##teamcity[" + format + "]", encodedArgs);
        }

        void Output(TestCompleted message)
        {
            if (!string.IsNullOrEmpty(message.Output))
                Message("testStdOut name='{0}' out='{1}'", message.Name, message.Output);
        }

        static string Encode(string? value)
        {
            if (value == null)
                return "";

            var builder = new StringBuilder();

            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '|': builder.Append("||"); break;
                    case '\'': builder.Append("|'"); break;
                    case '[': builder.Append("|["); break;
                    case ']': builder.Append("|]"); break;
                    case '\n': builder.Append("|n"); break;
                    case '\r': builder.Append("|r"); break;
                    default:
                        if (RequiresHexEscape(ch))
                        {
                            builder.Append("|0x");
                            builder.Append(((int) ch).ToString("x4"));
                        }
                        else
                        {
                            builder.Append(ch);
                        }

                        break;
                }
            }

            return builder.ToString();
        }

        static bool RequiresHexEscape(char ch)
            => ch > '\x007f';
    }
}