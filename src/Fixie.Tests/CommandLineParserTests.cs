using System;
using Should;

namespace Fixie.Tests
{
    public class CommandLineParserTests
    {
        public void EmptyByDefault()
        {
            var parser = new CommandLineParser();
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.CustomOptions.ShouldBeEmpty();
        }

        public void ParsesAssemblyPathsList()
        {
            var parser = new CommandLineParser("foo.dll", "bar.dll");
            parser.AssemblyPaths.ShouldEqual("foo.dll", "bar.dll");
            parser.CustomOptions.ShouldBeEmpty();
        }

        public void ParsesCustomOptions()
        {
            var parser = new CommandLineParser("--key", "value");
            parser.AssemblyPaths.ShouldBeEmpty();
            parser.CustomOptions.Count.ShouldEqual(1);
            parser.CustomOptions["key"].ShouldEqual("value");
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

        public void DemandsThatCustomOptionKeysMustBeUnique()
        {
            Action duplicatedKey = () => new CommandLineParser("--a", "1", "--b", "2", "--a", "2");

            duplicatedKey.ShouldThrow<Exception>("Option --a was specified twice.");
        }

        public void ParsesAssemblyPathsMixedWithCustomOptions()
        {
            var parser = new CommandLineParser("a.dll", "b.dll", "--oops", "c.dll", "d.dll", "--mode", "integration", "e.dll");

            parser.AssemblyPaths.ShouldEqual("a.dll", "b.dll", "d.dll", "e.dll");

            parser.CustomOptions.Count.ShouldEqual(2);
            parser.CustomOptions["oops"].ShouldEqual("c.dll");
            parser.CustomOptions["mode"].ShouldEqual("integration");
        }
    }
}