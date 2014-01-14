using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixie.Listeners
{
    public class NUnit2XmlOutputListener : Listener
    {
        readonly TextWriter writer;
        readonly XElement testResultsElement;
        DateTime startTime;
        readonly Dictionary<Type, XElement> testSuiteDictionary = new Dictionary<Type, XElement> ();
        readonly Dictionary<Type, TimeSpan> testSuiteDurations = new Dictionary<Type, TimeSpan> (); 

        public NUnit2XmlOutputListener (TextWriter writer)
        {
            this.writer = writer;
            testResultsElement =
                new XElement ("test-results",
                              new XElement("test-suite", new XAttribute("success", "True"), new XElement("results")));
        }

        public NUnit2XmlOutputListener () : this(new StreamWriter("TestResults.xml"))
        {
        }

        public void AssemblyStarted (Assembly assembly)
        {
            startTime = DateTime.UtcNow;
            testResultsElement.SetAttributeValue ("date", startTime.ToString("yyyy-MM-dd"));
            testResultsElement.SetAttributeValue ("time", startTime.ToString ("HH:mm:ss"));
            testResultsElement.SetAttributeValue ("name", assembly.Location);
            testResultsElement.Element("test-suite").SetAttributeValue("name", assembly.Location);
        }

        public void CaseSkipped (Case @case)
        {
            GetClassTestSuite (@case.Class)
                .Element ("results")
                .Add (new XElement ("test-case", new XAttribute ("name", @case.Name), new XAttribute ("executed", "False"), new XAttribute ("success", "True")));
        }

        public void CasePassed (PassResult result)
        {
            GetClassTestSuite (result.Case.Class)
                .Element ("results")
                .Add (new XElement ("test-case", new XAttribute ("name", result.Case.Name),
                    new XAttribute ("executed", "True"),
                    new XAttribute ("success", "True"),
                    new XAttribute ("time", result.Duration.TotalSeconds.ToString ("0.000"))));
        }

        public void CaseFailed (FailResult result)
        {
            testSuiteDurations[result.Case.Class] = GetClassDuration (result.Case.Class) + result.Duration;
            var suite = GetClassTestSuite (result.Case.Class);
            suite.SetAttributeValue("success", "False");
            testResultsElement.Element ("test-suite").SetAttributeValue ("success", "False");
            suite
                .Element ("results")
                .Add (new XElement ("test-case",
                    new XAttribute ("name", result.Case.Name),
                    new XElement("failure", new XElement("message", new XCData (result.PrimaryExceptionMessage ())), new XElement("stack-trace", new XCData (result.CompoundStackTrace ()))),
                    new XAttribute ("executed", "True"),
                    new XAttribute ("success", "False"),
                    new XAttribute ("time", result.Duration.TotalSeconds.ToString("0.000"))));
        }

        public void AssemblyCompleted (Assembly assembly, AssemblyResult result)
        {
            testResultsElement.SetAttributeValue ("total", result.Total);
            testResultsElement.SetAttributeValue ("failures", result.Failed);
            testResultsElement.SetAttributeValue ("not-run", result.Skipped);

            TimeSpan totalTimeSpan = TimeSpan.Zero;
            foreach (var entry in testSuiteDurations) {
                var suite = GetClassTestSuite (entry.Key);
                suite.SetAttributeValue ("time", entry.Value.TotalSeconds.ToString ("0.000"));
                totalTimeSpan += entry.Value;
            }

            testResultsElement.Element ("test-suite").SetAttributeValue ("time", totalTimeSpan.TotalSeconds.ToString ("0.000"));
            using (var xmlWriter = XmlWriter.Create (writer, new XmlWriterSettings () { CloseOutput = false,  })) {
                testResultsElement.WriteTo (xmlWriter);
                xmlWriter.Flush ();
            }

            writer.Flush ();
        }

        XElement GetClassTestSuite (Type @class)
        {
            XElement ret;
            if (!testSuiteDictionary.TryGetValue (@class, out ret)) {
                ret = new XElement("test-suite", new XElement("results"), new XAttribute("name", @class.FullName), new XAttribute("success", "True"), new XAttribute("time", ""));
                testResultsElement.Element("test-suite").Element("results").Add(ret);
                testSuiteDictionary[@class] = ret;
            }

            return ret;
        }

        TimeSpan GetClassDuration (Type @class)
        {
            TimeSpan duration;
            if (testSuiteDurations.TryGetValue (@class, out duration))
                return duration;

            return TimeSpan.Zero;
        }
    }
}
