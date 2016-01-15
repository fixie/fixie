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

        public void DemandsAtLeastOneAssemblyPath()
        {
            var parser = new CommandLineParser();
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Missing required test assembly path(s).");
        }

        public void ParsesExistingAssemblyPaths()
        {
            var parser = new CommandLineParser(assemblyPathA, assemblyPathB);
            parser.AssemblyPaths.ShouldEqual(assemblyPathA, assemblyPathB);
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void DemandsAssemblyPathsExist()
        {
            var parser = new CommandLineParser("foo.dll", "bar.dll");
            parser.AssemblyPaths.ShouldEqual("foo.dll", "bar.dll");
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Specified test assembly does not exist: foo.dll", "Specified test assembly does not exist: bar.dll");
        }

        public void ParsesNUnitXmlOutputFile()
        {
            var parser = new CommandLineParser(assemblyPathA, "--NUnitXml", "TestResult.xml");
            parser.AssemblyPaths.ShouldEqual(assemblyPathA);
            parser.Options.Keys.ShouldEqual("NUnitXml");
            parser.Options["NUnitXml"].ShouldEqual("TestResult.xml");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesXUnitXmlOutputFile()
        {
            var parser = new CommandLineParser(assemblyPathA, "--xUnitXml", "TestResult.xml");
            parser.AssemblyPaths.ShouldEqual(assemblyPathA);
            parser.Options.Keys.ShouldEqual("xUnitXml");
            parser.Options["xUnitXml"].ShouldEqual("TestResult.xml");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesTeamCityOutputFlag()
        {
            var parser = new CommandLineParser(assemblyPathA, "--TeamCity", "off");
            parser.AssemblyPaths.ShouldEqual(assemblyPathA);
            parser.Options.Keys.ShouldEqual("TeamCity");
            parser.Options["TeamCity"].ShouldEqual("off");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesOptions()
        {
            var parser = new CommandLineParser(assemblyPathA, "--key", "value", "--otherKey", "otherValue");
            parser.AssemblyPaths.ShouldEqual(assemblyPathA);
            parser.Options.Keys.ShouldEqual("key", "otherKey");
            parser.Options["key"].ShouldEqual("value");
            parser.Options["otherKey"].ShouldEqual("otherValue");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void DemandsThatOptionsHaveExplicitValues()
        {
            var parser = new CommandLineParser("--NUnitXml", "TestResult.xml", "--key");
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Options.Keys.ShouldEqual("NUnitXml");
            parser.Options["NUnitXml"].ShouldEqual("TestResult.xml");
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Option --key is missing its required value.");
        }

        public void DemandsThatOptionValuesCannotLookLikeKeys()
        {
            var parser = new CommandLineParser("--NUnitXml", "--anotherKey");
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Option --NUnitXml is missing its required value.");
        }

        public void ParsesAllOptionsValuesProvidedForEachOptionKey()
        {
            var parser = new CommandLineParser(assemblyPathA, "--a", "1", "--b", "2", "--a", "3");

            parser.AssemblyPaths.ShouldEqual(assemblyPathA);
            parser.Options.Keys.OrderBy(x => x).ShouldEqual("a", "b");
            parser.Options["a"].ShouldEqual("1", "3");
            parser.Options["b"].ShouldEqual("2");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesAssemblyPathsMixedWithOptionsOptions()
        {
            var parser = new CommandLineParser(assemblyPathA, "--include", "CategoryA", assemblyPathB, "--NUnitXml", "TestResult.xml", "--oops", "c.dll", assemblyPathA, "--include", "CategoryB", "--mode", "integration", assemblyPathB);

            parser.AssemblyPaths.ShouldEqual(assemblyPathA, assemblyPathB, assemblyPathA, assemblyPathB);

            parser.Options.Keys.OrderBy(x => x).ShouldEqual("include", "mode", "NUnitXml", "oops");
            parser.Options["include"].ShouldEqual("CategoryA", "CategoryB");
            parser.Options["mode"].ShouldEqual("integration");
            parser.Options["NUnitXml"].ShouldEqual("TestResult.xml");
            parser.Options["oops"].ShouldEqual("c.dll");
            parser.Options["nonexistent"].ShouldBeEmpty();
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }
    }
}