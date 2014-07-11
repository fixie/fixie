using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fixie.Conventions;
using Fixie.Discovery;

namespace Fixie.Tests.Discovery
{
    public class TestMethodDiscoveryTests
    {
        public void ShouldConsiderOnlyPublicInstanceMethods()
        {
            var customConvention = new Convention();

            DiscoveredTestMethods<Sample>(customConvention)
                .ShouldEqual(
                    "PublicInstanceNoArgsVoid",
                    "PublicInstanceNoArgsWithReturn",
                    "PublicInstanceWithArgsVoid",
                    "PublicInstanceWithArgsWithReturn");
        }

        public void ShouldDiscoverMethodsSatisfyingAllSpecifiedConditions()
        {
            var customConvention = new Convention();

            customConvention
                .Methods
                .Where(method => method.Name.Contains("Void"))
                .Where(method => method.Name.Contains("No"));

            DiscoveredTestMethods<Sample>(customConvention)
                .ShouldEqual("PublicInstanceNoArgsVoid");
        }

        public void CanDiscoverMethodsByNonInheritedAttributes()
        {
            var customConvention = new Convention();

            customConvention
                .Methods
                .Has<SampleAttribute>();

            DiscoveredTestMethods<Sample>(customConvention)
                .ShouldEqual("PublicInstanceWithArgsWithReturn");
        }

        public void CanDiscoverMethodsByInheritedAttributes()
        {
            var customConvention = new Convention();

            customConvention
                .Methods
                .HasOrInherits<SampleAttribute>();

            DiscoveredTestMethods<Sample>(customConvention)
                .ShouldEqual(
                    "PublicInstanceNoArgsWithReturn",
                    "PublicInstanceWithArgsWithReturn");
        }

        public void TheDefaultConventionShouldDiscoverSynchronousPublicInstanceVoidMethods()
        {
            var defaultConvention = new DefaultConvention();

            DiscoveredTestMethods<Sample>(defaultConvention)
                .ShouldEqual(
                    "PublicInstanceNoArgsVoid",
                    "PublicInstanceWithArgsVoid");
        }

        public void TheDefaultConventionShouldDiscoverAsyncPublicInstanceMethods()
        {
            var defaultConvention = new DefaultConvention();

            DiscoveredTestMethods<AsyncSample>(defaultConvention)
                .ShouldEqual(
                    "PublicInstanceNoArgsVoid",
                    "PublicInstanceNoArgsWithReturn",
                    "PublicInstanceWithArgsVoid",
                    "PublicInstanceWithArgsWithReturn");
        }

        static IEnumerable<string> DiscoveredTestMethods<TTestClass>(Convention convention)
        {
            return new CaseDiscoverer(convention.Config)
                .TestMethods(typeof(TTestClass))
                .OrderBy(method => method.Name, StringComparer.Ordinal)
                .Select(method => method.Name);
        }

        class SampleAttribute : Attribute { }

        class SampleBase
        {
            [Sample]
            public virtual int PublicInstanceNoArgsWithReturn() { return 0; }
        }

        class Sample : SampleBase, IDisposable
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

            public void Dispose() { }
        }

        class AsyncSample
        {
            public async static Task<int> PublicStaticWithArgsWithReturn(int x) { return await Zero(); }
            public async static Task<int> PublicStaticNoArgsWithReturn() { return await Zero(); }
            public async static void PublicStaticWithArgsVoid(int x) { await Zero(); }
            public async static void PublicStaticNoArgsVoid() { await Zero(); }

            public async Task<int> PublicInstanceWithArgsWithReturn(int x) { return await Zero(); }
            public async Task<int> PublicInstanceNoArgsWithReturn() { return await Zero(); }
            public async void PublicInstanceWithArgsVoid(int x) { await Zero(); }
            public async void PublicInstanceNoArgsVoid() { await Zero(); }

            private async static Task<int> PrivateStaticWithArgsWithReturn(int x) { return await Zero(); }
            private async static Task<int> PrivateStaticNoArgsWithReturn() { return await Zero(); }
            private async static void PrivateStaticWithArgsVoid(int x) { await Zero(); }
            private async static void PrivateStaticNoArgsVoid() { await Zero(); }

            private async Task<int> PrivateInstanceWithArgsWithReturn(int x) { return await Zero(); }
            private async Task<int> PrivateInstanceNoArgsWithReturn() { return await Zero(); }
            private async void PrivateInstanceWithArgsVoid(int x) { await Zero(); }
            private async void PrivateInstanceNoArgsVoid() { await Zero(); }

            static Task<int> Zero()
            {
                return Task.Run(() => 0);
            }
        }
    }
}