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
            parser.Keys.ShouldBeEmpty();
        }

        public void ParsesAssemblyPathsList()
        {
            var parser = new CommandLineParser("foo.dll", "bar.dll");
            parser.AssemblyPaths.ShouldEqual("foo.dll", "bar.dll");
            parser.Keys.ShouldBeEmpty();
        }

        public void ParsesCustomOptions()
        {
            var parser = new CommandLineParser("--key", "value");
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Keys.ShouldEqual("key");
            parser["key"].ShouldEqual("value");
        }

        public void DemandsThatCustomOptionsHaveExplicitValues()
        {
            Action keyWithoutValue = () => new CommandLineParser("--key", "value", "--invalid");

            keyWithoutValue.ShouldThrow<Exception>("Option --invalid is missing its required value.");
        }

        public void DemandsThatCustomOptionValuesCannotLookLikeKeys()
        {
            Action keyFollowedByAnotherKey = () => new CommandLineParser("--key", "--anotherKey");

            keyFollowedByAnotherKey.ShouldThrow<Exception>("Option --key is missing its required value.");
        }

        public void ParsesAllValuesProvidedForEachKey()
        {
            var parser = new CommandLineParser("--a", "1", "--b", "2", "--a", "3");

            parser.AssemblyPaths.ShouldBeEmpty();
            parser.Keys.OrderBy(x => x).ShouldEqual("a", "b");
            parser.GetAll("a").ShouldEqual("1", "3");
            parser["b"].ShouldEqual("2");
            parser.GetAll("b").ShouldEqual("2");
        }

        public void DemandsSingleValueForKeyWhenUsingIndexerForLookups()
        {
            var parser = new CommandLineParser("--a", "1", "--b", "2", "--a", "3");

            string a;
            string c;

            Action attemptIndexerForKeyWithMultipleValues = () => a = parser["a"];
            Action attemptIndexerForKeyWithZeroValues = () => c = parser["c"];

            attemptIndexerForKeyWithMultipleValues.ShouldThrow<ArgumentException>(
                "Option --a has multiple values. Instead of using the indexer " +
                "property, call GetAll(string) to retrieve all the values.");

            attemptIndexerForKeyWithZeroValues.ShouldThrow<ArgumentException>(
                "Option --c has no value. Instead of using the indexer " +
                "property for optional values, call GetAll(string) to retrieve " +
                "a possibly-empty collection of all the values.");
        }

        public void ParsesAssemblyPathsMixedWithCustomOptions()
        {
            var parser = new CommandLineParser("a.dll", "--include", "CategoryA", "b.dll", "--oops", "c.dll", "d.dll", "--include", "CategoryB", "--mode", "integration", "e.dll");

            parser.AssemblyPaths.ShouldEqual("a.dll", "b.dll", "d.dll", "e.dll");

            parser.Keys.OrderBy(x => x).ShouldEqual("include", "mode", "oops");
            parser["oops"].ShouldEqual("c.dll");
            parser["mode"].ShouldEqual("integration");
            parser.GetAll("include").ShouldEqual("CategoryA", "CategoryB");
        }
    }
}