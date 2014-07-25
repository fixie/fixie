using Fixie.Execution;
using Should;

namespace Fixie.Tests.Execution
{
    public class TraitFilterTests
    {
        readonly Trait[] noTraits = new Trait[] { };

        public void EmptyTraitCollectionShouldMatchEmptyFilter()
        {
            new TraitFilter(noTraits, noTraits).IsMatch(new Trait[] { }).ShouldBeTrue();
        }

        public void AnyTraitShouldMatchEmptyFilter()
        {
            new TraitFilter(noTraits, noTraits).IsMatch(new[] { new Trait("Number", "1") }).ShouldBeTrue();
        }

        public void OnlyIncludedTraitsShouldMatch()
        {
            var includedTraits = new[] { new Trait("Number", "1") };

            var traitFilter = new TraitFilter(includedTraits, noTraits);

            traitFilter.IsMatch(new[] { new Trait("Number", "1") }).ShouldBeTrue();
            traitFilter.IsMatch(new[] { new Trait("Number", "2") }).ShouldBeFalse();
        }

        public void AllButExcludedTraitsShouldMatch()
        {
            var excludedTraits = new[] { new Trait("Number", "1") };

            var traitFilter = new TraitFilter(noTraits, excludedTraits);

            traitFilter.IsMatch(new[] { new Trait("Number", "1") }).ShouldBeFalse();
            traitFilter.IsMatch(new[] { new Trait("Number", "2") }).ShouldBeTrue();
        }

        public void OnlyIncludedAndNotExcludedTraitsShouldMatch()
        {
            var includedTraits = new[] { new Trait("Category", "Foo") };
            var excludedTraits = new[] { new Trait("Number", "1") };

            var traitFilter = new TraitFilter(includedTraits, excludedTraits);

            traitFilter.IsMatch(new[] { new Trait("Category", "Foo") }).ShouldBeTrue();
            traitFilter.IsMatch(new[] { new Trait("Category", "Foo"), new Trait("Number", "1") }).ShouldBeFalse();
        }
    }
}