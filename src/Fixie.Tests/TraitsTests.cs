using Fixie.Conventions;
using Fixie.Discovery;
using Should;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Tests
{
    public class TraitsTests
    {
        public void ShouldDiscoverTraits()
        {
            var convention = new DefaultConvention();
            convention.Traits(method => method.GetCustomAttributes<TraitAttribute>()
                                              .Select(attribute => new Trait(attribute.Key, attribute.Value))
                                              .OrderBy(trait => trait.Value, StringComparer.Ordinal));

            var options = new OptionsBuilder().ToLookup();

            var caseDiscoverer = new CaseDiscoverer(convention.Config, options);

            var traitsByMethodName = caseDiscoverer.TestCases(typeof(TraitsTestClass))
                                                   .ToDictionary(@case => @case.Method.Name, @case => @case.Traits);

            traitsByMethodName["Method1"].Count.ShouldEqual(0);

            traitsByMethodName["Method2"].Count.ShouldEqual(1);
            traitsByMethodName["Method2"][0].Key.ShouldEqual("Category");
            traitsByMethodName["Method2"][0].Value.ShouldEqual("Foo");

            traitsByMethodName["Method3"].Count.ShouldEqual(2);
            traitsByMethodName["Method3"][0].Key.ShouldEqual("Category");
            traitsByMethodName["Method3"][0].Value.ShouldEqual("Bar");
            traitsByMethodName["Method3"][1].Key.ShouldEqual("Category");
            traitsByMethodName["Method3"][1].Value.ShouldEqual("Foo");
        }

        public void CanSpecifyTestsToInclueUsingTraits()
        {
            var convention = new DefaultConvention();
            convention.Traits(method => method.GetCustomAttributes<TraitAttribute>()
                                              .Select(attribute => new Trait(attribute.Key, attribute.Value)));

            var options = new OptionsBuilder()
                .Add(CommandLineOption.Include, "Category=Bar")
                .ToLookup();

            var caseDiscoverer = new CaseDiscoverer(convention.Config, options);

            caseDiscoverer.TestCases(typeof(TraitsTestClass))
                          .Select(@case => @case.Method.Name)
                          .ShouldEqual(new[] { "Method3" });
        }

        public void CanSpecifyTestsToExclueUsingTraits()
        {
            var convention = new DefaultConvention();
            convention.Traits(method => method.GetCustomAttributes<TraitAttribute>()
                                              .Select(attribute => new Trait(attribute.Key, attribute.Value)));

            var options = new OptionsBuilder()
                .Add(CommandLineOption.Exclude, "Category=Bar")
                .ToLookup();

            var caseDiscoverer = new CaseDiscoverer(convention.Config, options);

            caseDiscoverer.TestCases(typeof(TraitsTestClass))
                          .Select(@case => @case.Method.Name)
                          .ShouldEqual(new[] { "Method1", "Method2" });
        }

        private class TraitsTestClass
        {
            public void Method1() { }

            [Trait("Category", "Foo")]
            public void Method2() { }

            [Trait("Category", "Foo"), Trait("Category", "Bar")]
            public void Method3() { }
        }
    }
}