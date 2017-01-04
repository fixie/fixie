namespace Fixie.Tests.ConsoleRunner
{
    using System.Linq;
    using Fixie.ConsoleRunner;
    using Should;

    public class CommandLineParserTests
    {
        readonly string assemblyPath;

        public CommandLineParserTests()
        {
            assemblyPath = typeof(CommandLineParserTests).Assembly.Location;
        }

        public void ParsesExistingAssemblyPath()
        {
            var parser = new CommandLineParser(assemblyPath);
            parser.AssemblyPath.ShouldEqual(assemblyPath);
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void DemandsAssemblyPath()
        {
            var parser = new CommandLineParser();
            parser.AssemblyPath.ShouldBeNull();
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Missing required test assembly path.");
        }

        public void DemandsAtMostOneAssemblyPath()
        {
            var secondAssemblyPath = typeof(Case).Assembly.Location;

            var parser = new CommandLineParser(assemblyPath, secondAssemblyPath);
            parser.AssemblyPath.ShouldBeNull();
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Only one test assembly path may be specified.");
        }

        public void DemandsAssemblyPathsExist()
        {
            var parser = new CommandLineParser("foo.dll");
            parser.AssemblyPath.ShouldEqual("foo.dll");
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Specified test assembly does not exist: foo.dll");
        }

        public void DemandsAssemblyDirectoryContainsFixie()
        {
            var mscorlib = typeof(string).Assembly.Location;
            var parser = new CommandLineParser(mscorlib);
            parser.AssemblyPath.ShouldEqual(mscorlib);
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual($"Specified assembly {mscorlib} does not appear to be a test assembly. Ensure that it references Fixie.dll and try again.");
        }

        public void ParsesNUnitXmlReportFormat()
        {
            var parser = new CommandLineParser(assemblyPath, "--ReportFormat", "NUnit");
            parser.AssemblyPath.ShouldEqual(assemblyPath);
            parser.Options.Keys.ShouldEqual("ReportFormat");
            parser.Options["ReportFormat"].ShouldEqual("NUnit");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesXUnitXmlReportFormat()
        {
            var parser = new CommandLineParser(assemblyPath, "--ReportFormat", "xUnit");
            parser.AssemblyPath.ShouldEqual(assemblyPath);
            parser.Options.Keys.ShouldEqual("ReportFormat");
            parser.Options["ReportFormat"].ShouldEqual("xUnit");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void DemandsSupportedReportFormat()
        {
            var parser = new CommandLineParser(assemblyPath, "--ReportFormat", "unsupported");
            parser.AssemblyPath.ShouldEqual(assemblyPath);
            parser.Options.Keys.ShouldEqual("ReportFormat");
            parser.Options["ReportFormat"].ShouldEqual("unsupported");
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("The specified report format, 'unsupported', is not supported.");
        }

        public void DemandsAtMostOneReportFormat()
        {
            var parser = new CommandLineParser(assemblyPath, "--ReportFormat", "NUnit", "--ReportFormat", "xUnit");
            parser.AssemblyPath.ShouldEqual(assemblyPath);
            parser.Options.Keys.ShouldEqual("ReportFormat");
            parser.Options["ReportFormat"].ShouldEqual("NUnit", "xUnit");
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("To avoid writing multiple reports over the same file, only one report format may be specified.");
        }

        public void ParsesTeamCityOutputFlag()
        {
            var parser = new CommandLineParser(assemblyPath, "--TeamCity", "off");
            parser.AssemblyPath.ShouldEqual(assemblyPath);
            parser.Options.Keys.ShouldEqual("TeamCity");
            parser.Options["TeamCity"].ShouldEqual("off");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesOptions()
        {
            var parser = new CommandLineParser(assemblyPath, "--key", "value", "--otherKey", "otherValue");
            parser.AssemblyPath.ShouldEqual(assemblyPath);
            parser.Options.Keys.ShouldEqual("key", "otherKey");
            parser.Options["key"].ShouldEqual("value");
            parser.Options["otherKey"].ShouldEqual("otherValue");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void DemandsThatOptionsHaveExplicitValues()
        {
            var parser = new CommandLineParser(assemblyPath, "--NUnitXml", "TestResult.xml", "--key");
            parser.AssemblyPath.ShouldEqual(assemblyPath);
            parser.Options.Keys.ShouldEqual("NUnitXml");
            parser.Options["NUnitXml"].ShouldEqual("TestResult.xml");
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Option --key is missing its required value.");
        }

        public void DemandsThatOptionValuesCannotLookLikeKeys()
        {
            var parser = new CommandLineParser(assemblyPath, "--NUnitXml", "--anotherKey");
            parser.AssemblyPath.ShouldEqual(assemblyPath);
            parser.Options.Count.ShouldEqual(0);
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Option --NUnitXml is missing its required value.");
        }

        public void ParsesAllOptionsValuesProvidedForEachOptionKey()
        {
            var parser = new CommandLineParser(assemblyPath, "--a", "1", "--b", "2", "--a", "3");

            parser.AssemblyPath.ShouldEqual(assemblyPath);
            parser.Options.Keys.OrderBy(x => x).ShouldEqual("a", "b");
            parser.Options["a"].ShouldEqual("1", "3");
            parser.Options["b"].ShouldEqual("2");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesAssemblyPathMixedWithOptions()
        {
            var parser = new CommandLineParser("--include", "CategoryA", "--NUnitXml", "TestResult.xml", "--oops", "c.dll", assemblyPath, "--include", "CategoryB", "--mode", "integration");

            parser.AssemblyPath.ShouldEqual(assemblyPath);

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