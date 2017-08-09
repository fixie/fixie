namespace Fixie.Execution.Listeners
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml.Linq;

    public class ClassReport
    {
        readonly List<XElement> cases;
        readonly ExecutionSummary summary;

        public ClassReport(Type @class)
        {
            Class = @class;
            cases = new List<XElement>();
            summary = new ExecutionSummary();
        }

        public void Add(CaseCompleted message)
        {
            cases.Add(Case(message));
            summary.Add(message);
        }

        public Type Class { get; }

        public IReadOnlyList<XElement> Cases => cases;

        public int Passed => summary.Passed;
        public int Failed => summary.Failed;
        public int Skipped => summary.Skipped;
        public TimeSpan Duration => summary.Duration;

        static XElement Case(CaseCompleted message)
        {
            if (message is CaseSkipped skipped)
                return Case(skipped);

            if (message is CasePassed passed)
                return Case(passed);

            if (message is CaseFailed failed)
                return Case(failed);

            return null;
        }

        static XElement Case(CaseSkipped message)
            => new XElement("test",
                new XAttribute("name", message.Name),
                new XAttribute("type", message.Class.FullName),
                new XAttribute("method", message.Method.Name),
                new XAttribute("result", "Skip"),
                message.Reason != null
                    ? new XElement("reason", new XElement("message", new XCData(message.Reason)))
                    : null);

        static XElement Case(CasePassed message)
            => new XElement("test",
                new XAttribute("name", message.Name),
                new XAttribute("type", message.Class.FullName),
                new XAttribute("method", message.Method.Name),
                new XAttribute("result", "Pass"),
                new XAttribute("time", Seconds(message.Duration)));

        static XElement Case(CaseFailed message)
            => new XElement("test",
                new XAttribute("name", message.Name),
                new XAttribute("type", message.Class.FullName),
                new XAttribute("method", message.Method.Name),
                new XAttribute("result", "Fail"),
                new XAttribute("time", Seconds(message.Duration)),
                Failure(message.Exception));


        static XElement Failure(CompoundException exception)
        {
            return new XElement("failure",
                new XAttribute("exception-type", exception.Type),
                new XElement("message", new XCData(exception.Message)),
                new XElement("stack-trace", new XCData(exception.StackTrace)));
        }

        static string Seconds(TimeSpan duration)
        {
            //The XML Schema spec requires decimal values to use a culture-ignorant format.
            return duration.TotalSeconds.ToString("0.000", NumberFormatInfo.InvariantInfo);
        }
    }
}