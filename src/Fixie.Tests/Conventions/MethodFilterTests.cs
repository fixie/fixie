using System;
using System.Linq;
using Fixie.Conventions;

namespace Fixie.Tests.Conventions
{
    public class MethodFilterTests
    {
        public void ConsidersOnlyPublicInstanceMethods()
        {
            new MethodFilter()
                .Filter(typeof(Sample))
                .OrderBy(method => method.Name)
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceNoArgsVoid", "PublicInstanceNoArgsWithReturn",
                             "PublicInstanceWithArgsVoid", "PublicInstanceWithArgsWithReturn");
        }

        public void ShouldFilterByAllSpecifiedConditions()
        {
            new MethodFilter()
                .Where(method => method.Name.Contains("Void"))
                .Where(method => method.Name.Contains("No"))
                .Filter(typeof(Sample))
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceNoArgsVoid");
        }

        public void CanFilterToMethodsWithAttributes()
        {
            new MethodFilter()
                .Has<SampleAttribute>()
                .Filter(typeof(Sample))
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceWithArgsWithReturn");

            new MethodFilter()
                .HasOrInherits<SampleAttribute>()
                .Filter(typeof(Sample))
                .OrderBy(method => method.Name)
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceNoArgsWithReturn", "PublicInstanceWithArgsWithReturn");
        }

        public void CanBeShuffled()
        {
            new MethodFilter()
                .Shuffle(new Random(0))
                .Filter(typeof(Sample))
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceWithArgsWithReturn", "PublicInstanceNoArgsWithReturn",
                             "PublicInstanceNoArgsVoid", "PublicInstanceWithArgsVoid");

            new MethodFilter()
                .Shuffle(new Random(1))
                .Filter(typeof(Sample))
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceNoArgsWithReturn", "PublicInstanceWithArgsVoid",
                             "PublicInstanceNoArgsVoid", "PublicInstanceWithArgsWithReturn");
        }

        public void CanBeSorted()
        {
            new MethodFilter()
                .Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal))
                .Filter(typeof(Sample))
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceNoArgsVoid", "PublicInstanceNoArgsWithReturn",
                             "PublicInstanceWithArgsVoid", "PublicInstanceWithArgsWithReturn");

            new MethodFilter()
                .Sort((x, y) => x.Name.Length.CompareTo(y.Name.Length))
                .Filter(typeof(Sample))
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceNoArgsVoid",
                             "PublicInstanceWithArgsVoid",
                             "PublicInstanceNoArgsWithReturn",
                             "PublicInstanceWithArgsWithReturn");
        }

        class SampleAttribute : Attribute { }

        class SampleBase
        {
            [Sample]
            public virtual int PublicInstanceNoArgsWithReturn() { return 0; }
        }

        class Sample : SampleBase
        {
            public static int PublicStaticWithArgsWithReturn(int x) { return 0; }
            public static int PublicStaticNoArgsWithReturn() { return 0; }
            public static void PublicStaticWithArgsVoid(int x) { }
            public static void PublicStaticNoArgsVoid() { }

            [Sample]
            public int PublicInstanceWithArgsWithReturn(int x) { return 0; }
            public override int PublicInstanceNoArgsWithReturn() { return 0; }
            public void PublicInstanceWithArgsVoid(int x) { }
            public void PublicInstanceNoArgsVoid() { }

            private static int PrivateStaticWithArgsWithReturn(int x) { return 0; }
            private static int PrivateStaticNoArgsWithReturn() { return 0; }
            private static void PrivateStaticWithArgsVoid(int x) { }
            private static void PrivateStaticNoArgsVoid() { }

            private int PrivateInstanceWithArgsWithReturn(int x) { return 0; }
            private int PrivateInstanceNoArgsWithReturn() { return 0; }
            private void PrivateInstanceWithArgsVoid(int x) { }
            private void PrivateInstanceNoArgsVoid() { }
        }
    }
}