namespace Fixie.Tests
{
    using Assertions;

    public class OptionsTests
    {
        public void ShouldBeEmptyUponConstruction()
        {
            var empty = new Options();

            empty.Count.ShouldEqual(0);
            empty.Keys.ShouldBeEmpty();
        }

        public void ShouldAllowLookupOfAllAddedValuesForEachKey()
        {
            var lookup = new Options();

            lookup.Add("A", "A1");
            lookup.Add("B", "B1");
            lookup.Add("A", "A2");
            lookup.Add("A", "A3");

            lookup.Count.ShouldEqual(2);
            lookup.Keys.ShouldEqual("A", "B");
            lookup.Contains("A").ShouldBeTrue();
            lookup.Contains("B").ShouldBeTrue();
            lookup.Contains("C").ShouldBeFalse();
            lookup["A"].ShouldEqual("A1", "A2", "A3");
            lookup["B"].ShouldEqual("B1");
        }

        public void ShouldReturnEmptyCollectionForUndefinedKeys()
        {
            var lookup = new Options();
            lookup["undefined key"].ShouldBeEmpty();
        }
    }
}
