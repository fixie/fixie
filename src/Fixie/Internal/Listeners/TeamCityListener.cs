namespace Fixie.Internal.Listeners
{
    using System;
    using System.Linq;
    using System.Text;
    using Internal;
    using static System.Environment;

    class TeamCityListener :
        Handler<AssemblyStarted>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>,
        Handler<AssemblyCompleted>
    {
        internal static TeamCityListener? Create()
        {
            if (GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null)
                return new TeamCityListener();

            return null;
        }

        public void Handle(AssemblyStarted message)
        {
            Message("testSuiteStarted name='{0}'", message.Assembly.GetName().Name);
        }

        public void Handle(CaseSkipped message)
        {
            TestStarted(message);
            Output(message);
            Message("testIgnored name='{0}' message='{1}'", message.Name, message.Reason);
            TestFinished(message);
        }

        public void Handle(CasePassed message)
        {
            TestStarted(message);
            Output(message);
            TestFinished(message);
        }

        public void Handle(CaseFailed message)
        {
            var details =
                message.Exception.GetType().FullName +
                NewLine +
                message.Exception.LiterateStackTrace();

            TestStarted(message);
            Output(message);
            Message("testFailed name='{0}' message='{1}' details='{2}'", message.Name, message.Exception.Message, details);
            TestFinished(message);
        }

        public void Handle(AssemblyCompleted message)
        {
            Message("testSuiteFinished name='{0}'", message.Assembly.GetName().Name);
        }

        static void TestStarted(CaseCompleted message)
        {
            Message("testStarted name='{0}'", message.Name);
        }

        static void TestFinished(CaseCompleted message)
        {
            Message("testFinished name='{0}' duration='{1}'", message.Name, DurationInMilliseconds(message.Duration));
        }

        static void Message(string format, params string?[] args)
        {
            var encodedArgs = args.Select(Encode).Cast<object>().ToArray();
            Console.WriteLine("##teamcity[" + format + "]", encodedArgs);
        }

        static void Output(CaseCompleted message)
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

        static string DurationInMilliseconds(TimeSpan duration)
            => $"{duration.TotalMilliseconds:0}";
    }
}