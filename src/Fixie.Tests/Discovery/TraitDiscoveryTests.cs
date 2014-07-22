using System.Reflection;
using Fixie.Conventions;
using Fixie.Discovery;
using Should;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Tests.Discovery
{
    public class TraitDiscoveryTests
    {
        readonly MethodInfo method;

        public TraitDiscoveryTests()
        {
            method = typeof(TraitDiscoveryTestClass).GetMethod("Method");
        }

        public void ShouldProvideZeroTraitsByDefault()
        {
            var convention = new Convention();

            DiscoveredTraits(convention).ShouldBeEmpty();
        }

        public void ShouldDiscoverTraits()
        {
            var convention = new Convention();

            convention
                .Traits
                .Add<TraitSource>();

            var traits = DiscoveredTraits(convention).ToList();

            traits.Count.ShouldEqual(1);
            traits[0].Key.ShouldEqual("Category");
            traits[0].Value.ShouldEqual("Test");
        }

        IEnumerable<Trait> DiscoveredTraits(Convention convention)
        {
            return new TraitDiscoverer(convention.Config).GetTraits(method);
        }

        class TraitDiscoveryTestClass
        {
            [Trait("Category", "Test")]
            public void Method() { }
        }

        class TraitSource : Fixie.TraitSource
        {
            public IEnumerable<Trait> GetTraits(MethodInfo method)
            {
                return method.GetCustomAttributes<TraitAttribute>()
                             .Select(attribute => new Trait(attribute.Key, attribute.Value))
                             .OrderBy(trait => trait.Value, StringComparer.Ordinal);
            }
        }
    }
}