using System;
using System.Linq;
using Should;

namespace Fixie.Tests
{
    public class CommandLineParserTests
    {
        public void EmptyByDefault()
        {
            var parser = new CommandLineParser();
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Options.ShouldBeEmpty();
        }

        public void ParsesAssemblyPathsList()
        {
            var parser = new CommandLineParser("foo.dll", "bar.dll");
            parser.AssemblyPaths.ShouldEqual("foo.dll", "bar.dll");
            parser.Options.ShouldBeEmpty();
        }

        public void ParsesCustomOptions()
        {
            var parser = new CommandLineParser("--key", "value");
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Options.Select(x => x.Key).ShouldEqual("key");
            parser.Options["key"].ShouldEqual("value");
        }

        public void DemandsThatCustomOptionsHaveExplicitValues()
        {
            Action keyWithoutValue = () => new CommandLineParser("--key", "value", "--invalid");

            keyWithoutValue.ShouldThrow<Exception>("Option 'invalid' is missing its required value.");
        }

        public void DemandsThatCustomOptionValuesCannotLookLikeKeys()
        {
            Action keyFollowedByAnotherKey = () => new CommandLineParser("--key", "--anotherKey");

            keyFollowedByAnotherKey.ShouldThrow<Exception>("Option 'key' is missing its required value.");
        }

        public void ParsesAllValuesProvidedForEachKey()
        {
            var parser = new CommandLineParser("--a", "1", "--b", "2", "--a", "3");

            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Options.Select(x => x.Key).OrderBy(x => x).ShouldEqual("a", "b");
            parser.Options["a"].ShouldEqual("1", "3");
            parser.Options["b"].ShouldEqual("2");
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
        }
    }
}