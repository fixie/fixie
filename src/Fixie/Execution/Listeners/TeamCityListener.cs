namespace Fixie.Execution.Listeners
{
    using System;
    using System.Linq;
    using System.Text;
    using Execution;
    using static System.Environment;

    public class TeamCityListener :
        Handler<AssemblyStarted>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>,
        Handler<AssemblyCompleted>
    {
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
            TestStarted(message);
            Output(message);
            var details = message.Exception.GetType().FullName + NewLine + message.StackTrace;
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

        static void Message(string format, params string[] args)
        {
            var encodedArgs = args.Select(Encode).Cast<object>().ToArray();
            Console.WriteLine("##teamcity[" + format + "]", encodedArgs);
        }

        static void Output(CaseCompleted message)
        {
            if (!String.IsNullOrEmpty(message.Output))
                Message("testStdOut name='{0}' out='{1}'", message.Name, message.Output);
        }

        static string Encode(string value)
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
                    case '\n': builder.Append("|n"); break; // Line Feed
                    case '\r': builder.Append("|r"); break; // Carriage Return
                    case '\u0085': builder.Append("|x"); break; // Next Line
                    case '\u2028': builder.Append("|l"); break; // Line Separator
                    case '\u2029': builder.Append("|p"); break; // Paragraph Separator
                    default: builder.Append(ch); break;
                }
            }

            return builder.ToString();
        }

        static string DurationInMilliseconds(TimeSpan duration)
        {
            return ((int)Math.Ceiling(duration.TotalMilliseconds)).ToString();
        }
    }
}