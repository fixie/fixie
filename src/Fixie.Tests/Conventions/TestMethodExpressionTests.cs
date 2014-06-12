using System.Collections.Generic;
using System.Reflection;
using Fixie.Conventions;
using System;
using System.Linq;

namespace Fixie.Tests.Conventions
{
    public class TestMethodExpressionTests
    {
        readonly Convention convention;

        public TestMethodExpressionTests()
        {
            convention = new Convention();
        }

        public void ConsidersOnlyPublicInstanceMethods()
        {
            DiscoveredTestMethods(typeof(Sample))
                .OrderBy(method => method.Name, StringComparer.Ordinal)
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceNoArgsVoid", "PublicInstanceNoArgsWithReturn",
                             "PublicInstanceWithArgsVoid", "PublicInstanceWithArgsWithReturn");
        }

        public void ShouldFilterByAllSpecifiedConditions()
        {
            convention
                .Methods
                .Where(method => method.Name.Contains("Void"))
                .Where(method => method.Name.Contains("No"));

            DiscoveredTestMethods(typeof(Sample))
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceNoArgsVoid");
        }

        public void CanFilterToMethodsWithNonInheritedAttributes()
        {
            convention
                .Methods
                .Has<SampleAttribute>();

            DiscoveredTestMethods(typeof(Sample))
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceWithArgsWithReturn");
        }

        public void CanFilterToMethodsWithInheritedAttributes()
        {
            convention
                .Methods
                .HasOrInherits<SampleAttribute>();

            DiscoveredTestMethods(typeof(Sample))
                .OrderBy(method => method.Name, StringComparer.Ordinal)
                .Select(method => method.Name)
                .ShouldEqual("PublicInstanceNoArgsWithReturn", "PublicInstanceWithArgsWithReturn");
        }

        IEnumerable<MethodInfo> DiscoveredTestMethods(Type testClass)
        {
            return new DiscoveryModel(convention.Config)
                .TestMethods(testClass);
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