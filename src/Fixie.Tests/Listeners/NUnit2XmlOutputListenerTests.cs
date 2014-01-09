using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using Fixie.Conventions;
using Fixie.Listeners;
using Should;

namespace Fixie.Tests.Listeners
{
	public class NUnit2XmlOutputListenerTests
	{
		public void ShouldProduceAValidXmlOutput ()
		{
			var stream = new MemoryStream ();
			var listener = new NUnit2XmlOutputListener (new StreamWriter (stream));
			var runner = new Runner (listener);

			runner.RunType (GetType ().Assembly, new SelfTestConvention (), typeof(PassFailTestClass));
			stream.Position = 0;

			var doc = XDocument.Load (stream);
//			Console.WriteLine(doc);

			var schemaSet = new XmlSchemaSet ();
			schemaSet.Add (null, XmlReader.Create (Path.Combine ("Listeners", "NUnit2Results.xsd")));
			doc.Validate (schemaSet, null);

			var root = doc.Root;
			root.Attribute ("failures").Value.ShouldEqual ("2");
			root.Attribute ("total").Value.ShouldEqual ("6");
			root.Attribute ("not-run").Value.ShouldEqual ("1");

			var testSuite = root.Elements ().FirstOrDefault ();
			testSuite.ShouldNotBeNull ();
			testSuite.Attribute ("name").Value.ShouldContain ("Fixie.Tests.dll");
			testSuite.Attribute ("success").Value.ShouldEqual ("False");

			var actualResults = testSuite.XPathSelectElement ("results/test-suite/results");

			// Times vary by test run, so clear them out before comparing.
			foreach (var c in actualResults.XPathSelectElements ("test-case[@time != '']"))
				c.SetAttributeValue("time", String.Empty);

			// We can't easily test the stack trace or failure message, so clear them here.
			foreach (var msg in actualResults.XPathSelectElements ("test-case/failure/message | test-case/failure/stack-trace")) {
				msg.RemoveAll ();
			}

//			Console.WriteLine(doc);
			XElement.DeepEquals (actualResults,
				new XElement ("results",
					new XElement ("test-case", new XAttribute ("name", "Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass.SkipA"),
						new XAttribute ("executed", "False"), new XAttribute ("success", "True")),
					new XElement ("test-case", new XAttribute ("name", "Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass.FailA"),
						new XAttribute ("executed", "True"), new XAttribute ("success", "False"), new XAttribute("time",""),
						new XElement("failure", new XElement("message"), new XElement("stack-trace"))),
					new XElement ("test-case", new XAttribute ("name", "Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass.FailB"),
						new XAttribute ("executed", "True"), new XAttribute ("success", "False"), new XAttribute ("time", ""),
						new XElement ("failure", new XElement ("message"), new XElement ("stack-trace"))),
					new XElement ("test-case", new XAttribute ("name", "Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass.PassA"),
						new XAttribute ("executed", "True"), new XAttribute ("success", "True"), new XAttribute ("time", "")),
					new XElement ("test-case", new XAttribute ("name", "Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass.PassB"),
						new XAttribute ("executed", "True"), new XAttribute ("success", "True"), new XAttribute ("time", "")),
					new XElement ("test-case", new XAttribute ("name", "Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass.PassC"),
						new XAttribute ("executed", "True"), new XAttribute ("success", "True"), new XAttribute ("time", ""))
				)).ShouldBeTrue ();
		}

		class PassFailTestClass
		{
			public void FailA ()
			{
				throw new FailureException ();
			}

			public void PassA () { }

			public void FailB ()
			{
				throw new FailureException ();
			}

			public void PassB () { }

			public void PassC () { }

			public void SkipA () { throw new ShouldBeUnreachableException (); }
		}
	}
}
