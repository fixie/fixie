using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Fixie.Results;

namespace Fixie.Reports
{
    public class XUnitXmlReport
    {
        public XDocument Transform(ExecutionResult executionResult)
        {
            var now = DateTime.UtcNow;
            var assemblyResult = executionResult.AssemblyResults.Single();

            var @assembly =
                new XElement("assembly",
                    new XAttribute("name", assemblyResult.Name),
                    new XAttribute("run-date", now.ToString("yyyy-MM-dd")),
                    new XAttribute("run-time", now.ToString("HH:mm:ss")),
                    new XAttribute("configFile", AppDomain.CurrentDomain.SetupInformation.ConfigurationFile),
                    new XAttribute("time", GetTime(assemblyResult.Duration)),
                    new XAttribute("total", assemblyResult.Total),
                    new XAttribute("passed", assemblyResult.Passed),
                    new XAttribute("failed", assemblyResult.Failed),
                    new XAttribute("skipped", assemblyResult.Skipped),
                    new XAttribute("environment", string.Format("{0}-bit .NET {1}",
                        Environment.Is64BitProcess ? "64" : "32",
                        Assembly.ReflectionOnlyLoadFrom(assemblyResult.Name).ImageRuntimeVersion)),
                    new XAttribute("test-framework", string.Format("fixie {0}", Assembly.GetExecutingAssembly().GetName().Version)));

            foreach (var classResult in assemblyResult.ConventionResults.SelectMany(x => x.ClassResults))
                @assembly.Add(ClassElement(classResult));

            return new XDocument(@assembly);
        }

        private static XElement ClassElement(ClassResult classResult)
        {
            var @class = new XElement("class",
                new XAttribute("time", GetTime(classResult.Duration)),
                new XAttribute("name", classResult.Name),
                new XAttribute("total", classResult.Failed + classResult.Passed + classResult.Skipped),
                new XAttribute("passed", classResult.Passed),
                new XAttribute("failed", classResult.Failed),
                new XAttribute("skipped", classResult.Skipped));

            foreach (var caseResult in classResult.CaseResults)
                @class.Add(TestElement(caseResult));

            return @class;
        }

        static string GetTime(TimeSpan duration)
        {
            return Math.Round(duration.TotalSeconds, 3).ToString("0.000");
        }

        private static XElement TestElement(CaseResult caseResult)
        {
            var name = caseResult.Name;
            var index = name.IndexOf("(");
            if (index != -1)
                name = name.Substring(0, index);
            index = name.LastIndexOf(".");
            var type = name.Substring(0, index);
            var method = name.Substring(index + 1);

            var test = new XElement("test",
                new XAttribute("name", caseResult.Name),
                new XAttribute("type", type),
                new XAttribute("method", method),
                new XAttribute("result", caseResult.Status == CaseStatus.Failed
                    ? "fail"
                    : caseResult.Status == CaseStatus.Passed
                        ? "pass"
                        : "skip"));

            if (caseResult.Status == CaseStatus.Skipped)
                test.Add(new XElement("reason", new XElement("message", caseResult.SkipReason ?? string.Empty)));

            if (caseResult.Status != CaseStatus.Skipped)
                test.Add(new XAttribute("time", GetTime(caseResult.Duration)));

            if (caseResult.Status == CaseStatus.Failed)
                test.Add(FailureElement(caseResult));

            return test;
        }

        private static XElement FailureElement(CaseResult caseResult)
        {
            return new XElement("failure",
                new XAttribute("exception-type", caseResult.ExceptionType),
                new XElement("message", caseResult.Message),
                new XElement("stack-trace", caseResult.StackTrace));
        }
    }
}