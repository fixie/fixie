using System.Linq;
using Should;

namespace Fixie.Tests
{
    public class CommandLineParserTests
    {
        public void DemandsAtLeastOneAssemblyPath()
        {
            var parser = new CommandLineParser();
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Options.ShouldBeEmpty();
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Missing required test assembly path(s).");
        }

        public void ParsesAssemblyPathsList()
        {
            var parser = new CommandLineParser("foo.dll", "bar.dll");
            parser.AssemblyPaths.ShouldEqual("foo.dll", "bar.dll");
            parser.Options.ShouldBeEmpty();
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesNUnitXmlOutputFile()
        {
            var parser = new CommandLineParser("assembly.dll", "--NUnitXml", "TestResult.xml");
            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Select(x => x.Key).ShouldEqual("NUnitXml");
            parser.Options["NUnitXml"].ShouldEqual("TestResult.xml");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesXUnitXmlOutputFile()
        {
            var parser = new CommandLineParser("assembly.dll", "--XUnitXml", "TestResult.xml");
            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Select(x => x.Key).ShouldEqual("XUnitXml");
            parser.Options["XUnitXml"].ShouldEqual("TestResult.xml");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesTeamCityOutputFlag()
        {
            var parser = new CommandLineParser("assembly.dll", "--TeamCity", "off");
            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Select(x => x.Key).ShouldEqual("TeamCity");
            parser.Options["TeamCity"].ShouldEqual("off");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesCustomOptions()
        {
            var parser = new CommandLineParser("assembly.dll", "--key", "value");
            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Select(x => x.Key).ShouldEqual("key");
            parser.Options["key"].ShouldEqual("value");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void DemandsThatCustomOptionsHaveExplicitValues()
        {
            var parser = new CommandLineParser("--key", "value", "--invalid");
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Options.Select(x => x.Key).ShouldEqual("key");
            parser.Options["key"].ShouldEqual("value");
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Option --invalid is missing its required value.");
        }

        public void DemandsThatCustomOptionValuesCannotLookLikeKeys()
        {
            var parser = new CommandLineParser("--key", "--anotherKey");
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Options.ShouldBeEmpty();
            parser.HasErrors.ShouldBeTrue();
            parser.Errors.ShouldEqual("Option --key is missing its required value.");
        }

        public void ParsesAllValuesProvidedForEachKey()
        {
            var parser = new CommandLineParser("assembly.dll", "--a", "1", "--b", "2", "--a", "3");

            parser.AssemblyPaths.ShouldEqual("assembly.dll");
            parser.Options.Select(x => x.Key).OrderBy(x => x).ShouldEqual("a", "b");
            parser.Options["a"].ShouldEqual("1", "3");
            parser.Options["b"].ShouldEqual("2");
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }

        public void ParsesAssemblyPathsMixedWithCustomOptions()
        {
            var parser = new CommandLineParser("a.dll", "--include", "CategoryA", "b.dll", "--oops", "c.dll", "d.dll", "--include", "CategoryB", "--mode", "integration", "e.dll");

            parser.AssemblyPaths.ShouldEqual("a.dll", "b.dll", "d.dll", "e.dll");

            parser.Options.Select(x => x.Key).OrderBy(x => x).ShouldEqual("include", "mode", "oops");
            parser.Options["include"].ShouldEqual("CategoryA", "CategoryB");
            parser.Options["oops"].ShouldEqual("c.dll");
            parser.Options["mode"].ShouldEqual("integration");
            parser.Options["nonexistent"].ShouldBeEmpty();
            parser.HasErrors.ShouldBeFalse();
            parser.Errors.ShouldBeEmpty();
        }
    }
}