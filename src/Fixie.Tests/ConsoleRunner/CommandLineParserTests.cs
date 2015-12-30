using System.Linq;
using Fixie.ConsoleRunner;
using Should;

namespace Fixie.Tests.ConsoleRunner
{
    public class CommandLineParserTests
    {
        readonly string assemblyPathA;
        readonly string assemblyPathB;

        public CommandLineParserTests()
        {
            assemblyPathA = typeof(CommandLineParserTests).Assembly.Location;
            assemblyPathB = typeof(Case).Assembly.Location;
        }

        public void DemandsAnAssemblyPath()
        {
            var parser = new CommandLineParser();
            parser.AssemblyPath.ShouldBeNull();
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Missing required test assembly path.");
        }

        public void DemandsOneAssemblyPath()
        {
            var parser = new CommandLineParser(assemblyPathA, assemblyPathB);
            parser.AssemblyPath.ShouldBeNull();
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Only one test assembly path is allowed. Invoke the test runner once per test assembly.");
        }

        public void DemandsAssemblyPathsExist()
        {
            var parser = new CommandLineParser("foo.dll");
            parser.AssemblyPath.ShouldEqual("foo.dll");
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Specified test assembly does not exist: foo.dll");
        }

        public void ParsesNUnitXmlOutputFile()
        {
            var parser = new CommandLineParser(assemblyPathA, "--NUnitXml", "TestResult.xml");
            parser.AssemblyPath.ShouldEqual(assemblyPathA);
            parser.Options.Keys.ShouldEqual("NUnitXml");
            parser.Options["NUnitXml"].ShouldEqual("TestResult.xml");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesXUnitXmlOutputFile()
        {
            var parser = new CommandLineParser(assemblyPathA, "--xUnitXml", "TestResult.xml");
            parser.AssemblyPath.ShouldEqual(assemblyPathA);
            parser.Options.Keys.ShouldEqual("xUnitXml");
            parser.Options["xUnitXml"].ShouldEqual("TestResult.xml");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesTeamCityOutputFlag()
        {
            var parser = new CommandLineParser(assemblyPathA, "--TeamCity", "off");
            parser.AssemblyPath.ShouldEqual(assemblyPathA);
            parser.Options.Keys.ShouldEqual("TeamCity");
            parser.Options["TeamCity"].ShouldEqual("off");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesOptions()
        {
            var parser = new CommandLineParser(assemblyPathA, "--key", "value", "--otherKey", "otherValue");
            parser.AssemblyPath.ShouldEqual(assemblyPathA);
            parser.Options.Keys.ShouldEqual("key", "otherKey");
            parser.Options["key"].ShouldEqual("value");
            parser.Options["otherKey"].ShouldEqual("otherValue");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void DemandsThatOptionsHaveExplicitValues()
        {
            var parser = new CommandLineParser("--NUnitXml", "TestResult.xml", "--key");
            parser.AssemblyPath.ShouldBeNull();
            parser.Options.Keys.ShouldEqual("NUnitXml");
            parser.Options["NUnitXml"].ShouldEqual("TestResult.xml");
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Option --key is missing its required value.");
        }

        public void DemandsThatOptionValuesCannotLookLikeKeys()
        {
            var parser = new CommandLineParser("--NUnitXml", "--anotherKey");
            parser.AssemblyPath.ShouldBeNull();
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Option --NUnitXml is missing its required value.");
        }

        public void ParsesAllOptionsValuesProvidedForEachOptionKey()
        {
            var parser = new CommandLineParser(assemblyPathA, "--a", "1", "--b", "2", "--a", "3");

            parser.AssemblyPath.ShouldEqual(assemblyPathA);
            parser.Options.Keys.OrderBy(x => x).ShouldEqual("a", "b");
            parser.Options["a"].ShouldEqual("1", "3");
            parser.Options["b"].ShouldEqual("2");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesAssemblyPathsMixedWithOptionsOptions()
        {
            var parser = new CommandLineParser("--include", "CategoryA", assemblyPathA, "--NUnitXml", "TestResult.xml", "--oops", "oops.dll", "--include", "CategoryB", "--mode", "integration");

            parser.AssemblyPath.ShouldEqual(assemblyPathA);

            parser.Options.Keys.OrderBy(x => x).ShouldEqual("include", "mode", "NUnitXml", "oops");
            parser.Options["include"].ShouldEqual("CategoryA", "CategoryB");
            parser.Options["mode"].ShouldEqual("integration");
            parser.Options["NUnitXml"].ShouldEqual("TestResult.xml");
            parser.Options["oops"].ShouldEqual("oops.dll");
            parser.Options["nonexistent"].ShouldBeEmpty();
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }
    }
}