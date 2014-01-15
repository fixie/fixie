using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Fixie.Conventions;
using Fixie.Listeners;
using Should;

namespace Fixie.Tests.Listeners
{
    public class NUnit2XmlOutputListenerTests
    {
        public void ShouldProduceAValidXmlOutput()
        {
            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream))
            {
                var listener = new NUnit2XmlOutputListener(streamWriter);
                var runner = new Runner(listener);

                runner.RunType(GetType().Assembly, new SelfTestConvention(), typeof(PassFailTestClass));

                stream.Position = 0;
                var actual = XDocument.Load(stream);

                XsdValidate(actual);
                CleanBrittleValues(actual.ToString(SaveOptions.DisableFormatting)).ShouldEqual(ExpectedReport);
            }
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by system date.
            var cleaned = Regex.Replace(actualRawContent, @"date=""\d\d\d\d-\d\d-\d\d""", @"date=""YYYY-MM-DD""");

            //Avoid brittle assertion introduced by system time.
            cleaned = Regex.Replace(cleaned, @"time=""\d\d:\d\d:\d\d""", @"time=""HH:MM:SS""");

            //Avoid brittle assertion introduced by test duration.
            cleaned = Regex.Replace(cleaned, @"time=""[\d\.]+""", @"time=""1.234""");

            //Avoid brittle assertion introduced by stack trace line numbers.
            cleaned = Regex.Replace(cleaned, @":line \d+", ":line #");

            return cleaned;
        }

        static void XsdValidate(XDocument doc)
        {
            var schemaSet = new XmlSchemaSet();
            using (var xmlReader = XmlReader.Create(Path.Combine("Listeners", "NUnit2Results.xsd")))
            {
                schemaSet.Add(null, xmlReader);
            }

            doc.Validate(schemaSet, null);
        }

        string ExpectedReport
        {
            get
            {
                var assemblyLocation = GetType().Assembly.Location;

                var expectedReport = @"<test-results date=""YYYY-MM-DD"" time=""HH:MM:SS"" name=""" + assemblyLocation + @""" total=""6"" failures=""2"" not-run=""1"">
  <test-suite success=""False"" name=""" + assemblyLocation + @""" time=""1.234"">
    <results>
      <test-suite name=""Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass"" success=""False"" time=""1.234"">
        <results>
          <test-case name=""Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass.SkipA"" executed=""False"" success=""True"" />
          <test-case name=""Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass.FailA"" executed=""True"" success=""False"" time=""1.234"">
            <failure>
              <message><![CDATA['FailA' failed!]]></message>
              <stack-trace><![CDATA['FailA' failed!
   at Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests.PassFailTestClass.FailA() in " + PathToThisFile() + @":line #]]></stack-trace>
            </failure>
          </test-case>
          <test-case name=""Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass.FailB"" executed=""True"" success=""False"" time=""1.234"">
            <failure>
              <message><![CDATA['FailB' failed!]]></message>
              <stack-trace><![CDATA['FailB' failed!
   at Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests.PassFailTestClass.FailB() in " + PathToThisFile() + @":line #]]></stack-trace>
            </failure>
          </test-case>
          <test-case name=""Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass.PassA"" executed=""True"" success=""True"" time=""1.234"" />
          <test-case name=""Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass.PassB"" executed=""True"" success=""True"" time=""1.234"" />
          <test-case name=""Fixie.Tests.Listeners.NUnit2XmlOutputListenerTests+PassFailTestClass.PassC"" executed=""True"" success=""True"" time=""1.234"" />
        </results>
      </test-suite>
    </results>
  </test-suite>
</test-results>";

                return XDocument.Parse(expectedReport).ToString(SaveOptions.DisableFormatting);
            }
        }

        static string PathToThisFile([CallerFilePath] string path = null)
        {
            return path;
        }

        class PassFailTestClass
        {
            public void FailA()
            {
                throw new FailureException();
            }

            public void PassA() { }

            public void FailB()
            {
                throw new FailureException();
            }

            public void PassB() { }

            public void PassC() { }

            public void SkipA()
            {
                throw new ShouldBeUnreachableException();
            }
        }
    }
}