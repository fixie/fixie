namespace Fixie.Tests.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;

    public class MethodDiscovererTests
    {
        class DefaultDiscovery : Discovery
        {
        }
        
        class MaximumDiscovery : Discovery
        {
            public IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
                => publicMethods;
        }

        class NarrowDiscovery : Discovery
        {
            public IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
            {
                return publicMethods
                    .Where(x => x.Name.Contains("Void"))
                    .Where(x => x.Name.Contains("No"))
                    .Where(x => !x.IsStatic);
            }
        }

        class BuggyDiscovery : Discovery
        {
            public IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
            {
                return publicMethods.Where(x => throw new Exception("Unsafe method discovery predicate threw!"));
            }
        }

        public void ShouldConsiderOnlyPublicMethods()
        {
            var discovery = new MaximumDiscovery();

            DiscoveredTestMethods<Sample>(discovery)
                .ShouldBe(
                    "PublicInstanceNoArgsVoid()",
                    "PublicInstanceNoArgsWithReturn()",
                    "PublicInstanceWithArgsVoid(x)",
                    "PublicInstanceWithArgsWithReturn(x)",

                    "PublicStaticNoArgsVoid()",
                    "PublicStaticNoArgsWithReturn()",
                    "PublicStaticWithArgsVoid(x)",
                    "PublicStaticWithArgsWithReturn(x)");

            DiscoveredTestMethods<AsyncSample>(discovery)
                .ShouldBe(
                    "PublicInstanceNoArgsVoid()",
                    "PublicInstanceNoArgsWithReturn()",
                    "PublicInstanceWithArgsVoid(x)",
                    "PublicInstanceWithArgsWithReturn(x)",

                    "PublicStaticNoArgsVoid()",
                    "PublicStaticNoArgsWithReturn()",
                    "PublicStaticWithArgsVoid(x)",
                    "PublicStaticWithArgsWithReturn(x)");
        }

        public void ShouldNotConsiderIDisposableOrIAsyncDisposableMethods()
        {
            var discovery = new MaximumDiscovery();

            DiscoveredTestMethods<DisposableSample>(discovery)
                .ShouldBe(
                    "Dispose(disposing)",
                    "DisposeAsync(disposing)",
                    "NotNamedDispose()");

            DiscoveredTestMethods<NonDisposableSample>(discovery)
                .ShouldBe(
                    "Dispose()",
                    "Dispose(disposing)",
                    "DisposeAsync()",
                    "DisposeAsync(disposing)",
                    "NotNamedDispose()");

            DiscoveredTestMethods<NonDisposableByReturnTypeSample>(discovery)
                .ShouldBe(
                    "Dispose()",
                    "Dispose(disposing)",
                    "DisposeAsync()",
                    "DisposeAsync(disposing)",
                    "NotNamedDispose()");
        }

        public void ShouldDiscoverMethodsSatisfyingAllSpecifiedConditions()
        {
            var discovery = new NarrowDiscovery();

            DiscoveredTestMethods<Sample>(discovery)
                .ShouldBe("PublicInstanceNoArgsVoid()");
        }

        public void TheDefaultDiscoveryShouldDiscoverPublicMethods()
        {
            var discovery = new DefaultDiscovery();

            DiscoveredTestMethods<Sample>(discovery)
                .ShouldBe(
                    "PublicInstanceNoArgsVoid()",
                    "PublicInstanceNoArgsWithReturn()",
                    "PublicInstanceWithArgsVoid(x)",
                    "PublicInstanceWithArgsWithReturn(x)",

                    "PublicStaticNoArgsVoid()",
                    "PublicStaticNoArgsWithReturn()",
                    "PublicStaticWithArgsVoid(x)",
                    "PublicStaticWithArgsWithReturn(x)");

            DiscoveredTestMethods<AsyncSample>(discovery)
                .ShouldBe(
                    "PublicInstanceNoArgsVoid()",
                    "PublicInstanceNoArgsWithReturn()",
                    "PublicInstanceWithArgsVoid(x)",
                    "PublicInstanceWithArgsWithReturn(x)",

                    "PublicStaticNoArgsVoid()",
                    "PublicStaticNoArgsWithReturn()",
                    "PublicStaticWithArgsVoid(x)",
                    "PublicStaticWithArgsWithReturn(x)");
        }

        public void ShouldFailWithClearExplanationWhenDiscoveryThrows()
        {
            var discovery = new BuggyDiscovery();

            Action attemptFaultyDiscovery = () => DiscoveredTestMethods<Sample>(discovery);

            var exception = attemptFaultyDiscovery.ShouldThrow<Exception>(
                "Exception thrown during test method discovery. " +
                "Check the inner exception for more details.");

            exception.InnerException
                .ShouldBe<Exception>()
                .Message.ShouldBe("Unsafe method discovery predicate threw!");
        }

        static IEnumerable<string> DiscoveredTestMethods<TTestClass>(Discovery discovery)
        {
            return new MethodDiscoverer(discovery)
                .TestMethods(typeof(TTestClass))
                .Select(method => $"{method.Name}({string.Join(", ", method.GetParameters().Select(x => x.Name))})")
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

        class AsyncSample : IAsyncDisposable
        {
            public static async Task<int> PublicStaticWithArgsWithReturn(int x) { return await Zero(); }
            public static async Task<int> PublicStaticNoArgsWithReturn() { return await Zero(); }
            public static async void PublicStaticWithArgsVoid(int x) { await Zero(); }
            public static async void PublicStaticNoArgsVoid() { await Zero(); }

            public async Task<int> PublicInstanceWithArgsWithReturn(int x) { return await Zero(); }
            public async Task<int> PublicInstanceNoArgsWithReturn() { return await Zero(); }
            public async void PublicInstanceWithArgsVoid(int x) { await Zero(); }
            public async void PublicInstanceNoArgsVoid() { await Zero(); }

            private static async Task<int> PrivateStaticWithArgsWithReturn(int x) { return await Zero(); }
            private static async Task<int> PrivateStaticNoArgsWithReturn() { return await Zero(); }
            private static async void PrivateStaticWithArgsVoid(int x) { await Zero(); }
            private static async void PrivateStaticNoArgsVoid() { await Zero(); }

            private async Task<int> PrivateInstanceWithArgsWithReturn(int x) { return await Zero(); }
            private async Task<int> PrivateInstanceNoArgsWithReturn() { return await Zero(); }
            private async void PrivateInstanceWithArgsVoid(int x) { await Zero(); }
            private async void PrivateInstanceNoArgsVoid() { await Zero(); }

            static Task<int> Zero()
            {
                return Task.Run(() => 0);
            }

            public ValueTask DisposeAsync() => default;
        }

        class DisposableSample : IAsyncDisposable, IDisposable
        {
            public ValueTask DisposeAsync(bool disposing) => default;
            public ValueTask DisposeAsync() => default;
            public void Dispose(bool disposing) { }
            public void Dispose() { }
            public void NotNamedDispose() { }
        }

        class NonDisposableSample
        {
            public ValueTask DisposeAsync(bool disposing) => default;
            public ValueTask DisposeAsync() => default;
            public void Dispose(bool disposing) { }
            public void Dispose() { }
            public void NotNamedDispose() { }
        }

        class NonDisposableByReturnTypeSample
        {
            public int DisposeAsync(bool disposing) => 0;
            public int DisposeAsync() => 0;
            public void Dispose(bool disposing) { }
            public int Dispose() => 0;
            public void NotNamedDispose() { }
        }
    }
}