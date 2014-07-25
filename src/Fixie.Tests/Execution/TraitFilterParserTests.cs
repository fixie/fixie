using System;
using System.Collections.Generic;
using System.Linq;
using Fixie.Execution;
using Should.Core.Assertions;

namespace Fixie.Tests.Execution
{
    public class TraitFilterParserTests
    {
        public void ShouldAcceptEmptyOptions()
        {
            Assert.DoesNotThrow(() => new TraitFilterParser().GetTraitFilter(OptionsBuilder.Empty));
        }

        public void ShouldAcceptCorrectIncludeOptions()
        {
            var options = new OptionsBuilder()
                .Add(CommandLineOption.Include, "key1=value1;key2=value2")
                .ToLookup();

            Assert.DoesNotThrow(() => new TraitFilterParser().GetTraitFilter(options));
        }

        public void ShouldThrowIfOnlyIncludeKeyIsSpecified()
        {
            var options = new OptionsBuilder()
                .Add(CommandLineOption.Include, "key")
                .ToLookup();

            Assert.Throws<FormatException>(() => new TraitFilterParser().GetTraitFilter(options))
                  .Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                  .ShouldEqual(new[]
                  {
                      "Invalid option 'Include key'.",
                      "Valid format is key=value[;key=value]."
                  });
        }

        public void ShouldThrowIfIncludeValueIsEmpty()
        {
            var options = new OptionsBuilder()
                .Add(CommandLineOption.Include, "key1=value1;key2=")
                .ToLookup();

            Assert.Throws<FormatException>(() => new TraitFilterParser().GetTraitFilter(options))
                  .Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                  .ShouldEqual(new[]
                  {
                      "Invalid option 'Include key1=value1;key2='.",
                      "Valid format is key=value[;key=value]."
                  });
        }

        public void ShouldThrowIfIncludeKeyIsEmpty()
        {
            var options = new OptionsBuilder()
                .Add(CommandLineOption.Include, "=value")
                .ToLookup();

            Assert.Throws<FormatException>(() => new TraitFilterParser().GetTraitFilter(options))
                  .Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                  .ShouldEqual(new[]
                  {
                      "Invalid option 'Include =value'.",
                      "Valid format is key=value[;key=value]."
                  });
        }

        public void ShouldAcceptCorrectExcludeOptions()
        {
            var options = new OptionsBuilder()
                .Add(CommandLineOption.Exclude, "key1=value1;key2=value2")
                .ToLookup();

            Assert.DoesNotThrow(() => new TraitFilterParser().GetTraitFilter(options));
        }

        public void ShouldThrowIfOnlyExcludeKeyIsSpecified()
        {
            var options = new OptionsBuilder()
                .Add(CommandLineOption.Exclude, "key")
                .ToLookup();

            Assert.Throws<FormatException>(() => new TraitFilterParser().GetTraitFilter(options))
                  .Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                  .ShouldEqual(new[]
                  {
                      "Invalid option 'Exclude key'.",
                      "Valid format is key=value[;key=value]."
                  });
        }

        public void ShouldThrowIfExcludeValueIsEmpty()
        {
            var options = new OptionsBuilder()
                .Add(CommandLineOption.Exclude, "key1=value1;key2=")
                .ToLookup();

            Assert.Throws<FormatException>(() => new TraitFilterParser().GetTraitFilter(options))
                  .Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                  .ShouldEqual(new[]
                  {
                      "Invalid option 'Exclude key1=value1;key2='.",
                      "Valid format is key=value[;key=value]."
                  });
        }

        public void ShouldThrowIfExcludeKeyIsEmpty()
        {
            var options = new OptionsBuilder()
                .Add(CommandLineOption.Exclude, "=value")
                .ToLookup();

            Assert.Throws<FormatException>(() => new TraitFilterParser().GetTraitFilter(options))
                  .Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                  .ShouldEqual(new[]
                  {
                      "Invalid option 'Exclude =value'.",
                      "Valid format is key=value[;key=value]."
                  });
        }

        class OptionsBuilder
        {
            readonly List<KeyValuePair<string, string>> options = new List<KeyValuePair<string, string>>();

            public static ILookup<string, string> Empty
            {
                get { return new OptionsBuilder().ToLookup(); }
            }

            public OptionsBuilder Add(string key, string value)
            {
                options.Add(new KeyValuePair<string, string>(key, value));
                return this;
            }

            public ILookup<string, string> ToLookup()
            {
                return options.ToLookup(x => x.Key, x => x.Value);
            }
        }
    }
}