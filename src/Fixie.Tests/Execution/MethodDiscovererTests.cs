namespace Fixie.Tests.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Assertions;
    using Conventions;
    using Fixie.Execution;

    public class MethodDiscovererTests
    {
        public void ShouldConsiderOnlyPublicInstanceMethods()
        {
            var customConvention = new Convention();

            DiscoveredTestMethods<Sample>(customConvention)
                .ShouldEqual(
                    "PublicInstanceNoArgsVoid()",
                    "PublicInstanceNoArgsWithReturn()",
                    "PublicInstanceWithArgsVoid(x)",
                    "PublicInstanceWithArgsWithReturn(x)");

            DiscoveredTestMethods<AsyncSample>(customConvention)
                .ShouldEqual(
                    "PublicInstanceNoArgsVoid()",
                    "PublicInstanceNoArgsWithReturn()",
                    "PublicInstanceWithArgsVoid(x)",
                    "PublicInstanceWithArgsWithReturn(x)");
        }

        public void ShouldNotConsiderIDisposableDisposeMethod()
        {
            var customConvention = new Convention();

            DiscoveredTestMethods<DisposableSample>(customConvention)
                .ShouldEqual(
                    "Dispose(disposing)",
                    "NotNamedDispose()");

            DiscoveredTestMethods<NonDisposableSample>(customConvention)
                .ShouldEqual(
                    "Dispose()",
                    "Dispose(disposing)",
                    "NotNamedDispose()");

            DiscoveredTestMethods<NonDisposableByReturnTypeSample>(customConvention)
                .ShouldEqual(
                    "Dispose()",
                    "Dispose(disposing)",
                    "NotNamedDispose()");
        }

        public void ShouldDiscoverMethodsSatisfyingAllSpecifiedConditions()
        {
            var customConvention = new Convention();

            customConvention
                .Methods
                .Where(x => x.Name.Contains("Void"))
                .Where(x => x.Name.Contains("No"));

            DiscoveredTestMethods<Sample>(customConvention)
                .ShouldEqual("PublicInstanceNoArgsVoid()");
        }

        public void ShouldDiscoverMethodsSatisfyingAnyPatternWhenPatternsAreProvided()
        {
            var customConvention = new Convention();

            DiscoveredTestMethods<Sample>(customConvention,
                "NOMATCH")
                .ShouldBeEmpty();

            DiscoveredTestMethods<Sample>(customConvention,
                typeof(Sample).FullName + ".Public")
                .ShouldEqual(
                    "PublicInstanceNoArgsVoid()",
                    "PublicInstanceNoArgsWithReturn()",
                    "PublicInstanceWithArgsVoid(x)",
                    "PublicInstanceWithArgsWithReturn(x)");

            DiscoveredTestMethods<Sample>(customConvention,
                    "PublicInstanceWith")
                .ShouldEqual(
                    "PublicInstanceWithArgsVoid(x)",
                    "PublicInstanceWithArgsWithReturn(x)");

            DiscoveredTestMethods<Sample>(customConvention,
                    "Void")
                .ShouldEqual(
                    "PublicInstanceNoArgsVoid()",
                    "PublicInstanceWithArgsVoid(x)");

            DiscoveredTestMethods<Sample>(customConvention,
                    "Public*With")
                .ShouldEqual(
                    "PublicInstanceNoArgsWithReturn()",
                    "PublicInstanceWithArgsVoid(x)",
                    "PublicInstanceWithArgsWithReturn(x)");
        }

        public void TheDefaultConventionShouldDiscoverPublicInstanceMethods()
        {
            var defaultConvention = new DefaultConvention();

            DiscoveredTestMethods<Sample>(defaultConvention)
                .ShouldEqual(
                    "PublicInstanceNoArgsVoid()",
                    "PublicInstanceNoArgsWithReturn()",
                    "PublicInstanceWithArgsVoid(x)",
                    "PublicInstanceWithArgsWithReturn(x)");

            DiscoveredTestMethods<AsyncSample>(defaultConvention)
                .ShouldEqual(
                    "PublicInstanceNoArgsVoid()",
                    "PublicInstanceNoArgsWithReturn()",
                    "PublicInstanceWithArgsVoid(x)",
                    "PublicInstanceWithArgsWithReturn(x)");
        }

        public void ShouldFailWithClearExplanationWhenAnyGivenConditionThrows()
        {
            var customConvention = new Convention();

            customConvention
                .Methods
                .Where(x => throw new Exception("Unsafe method-discovery predicate threw!"));

            Action attemptFaultyDiscovery = () => DiscoveredTestMethods<Sample>(customConvention);

            var exception = attemptFaultyDiscovery.ShouldThrow<Exception>(
                "Exception thrown while attempting to run a custom method-discovery predicate. " +
                "Check the inner exception for more details.");

            exception.InnerException.Message.ShouldEqual("Unsafe method-discovery predicate threw!");
        }

        static IEnumerable<string> DiscoveredTestMethods<TTestClass>(Convention convention, params string[] patterns)
        {
            var filter = new Filter();
            filter.ByPatterns(patterns);
            return new MethodDiscoverer(filter, convention)
                .TestMethods(typeof(TTestClass))
                .Select(method => $"{method.Name}({String.Join(", ", method.GetParameters().Select(x => x.Name))})")
                .OrderBy(name => name, StringComparer.Ordinal);
        }

        class SampleBase
        {
            public virtual int PublicInstanceNoArgsWithReturn() { return 0; }
        }

        class Sample : SampleBase, IDisposable
        {
            public static int PublicStaticWithArgsWithReturn(int x) { return 0; }
            public static int PublicStaticNoArgsWithReturn() { return 0; }
            public static void PublicStaticWithArgsVoid(int x) { }
            public static void PublicStaticNoArgsVoid() { }

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

        class DisposableSample : IDisposable
        {
            public void Dispose(bool disposing) { }
            public void Dispose() { }
            public void NotNamedDispose() { }
        }

        class NonDisposableSample
        {
            public void Dispose(bool disposing) { }
            public void Dispose() { }
            public void NotNamedDispose() { }
        }

        class NonDisposableByReturnTypeSample
        {
            public void Dispose(bool disposing) { }
            public int Dispose() => 0;
            public void NotNamedDispose() { }
        }
    }
}