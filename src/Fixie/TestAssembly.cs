namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Internal;

    public class TestAssembly
    {
        internal TestAssembly(Assembly assembly, ImmutableHashSet<string> selectedTests)
        {
            Assembly = assembly;
            SelectedTests = selectedTests;
        }

        internal Assembly Assembly { get; }

        /// <summary>
        /// Gets the set of explicitly selected test names to be executed.
        /// Empty under normal test execution when all tests are being executed.
        /// </summary>
        public ImmutableHashSet<string> SelectedTests { get; }

        internal async Task RunAsync(
            ImmutableHashSet<string> selectedTests,
            ExecutionRecorder recorder,
            IReadOnlyList<Type> classes,
            MethodDiscoverer methodDiscoverer,
            Execution execution)
        {
            foreach (var @class in classes)
            {
                IEnumerable<MethodInfo> methods = methodDiscoverer.TestMethods(@class);

                if (!selectedTests.IsEmpty)
                    methods = methods.Where(method => selectedTests.Contains(new Test(method).Name));

                var classIsDisposable = IsDisposable(@class);
                var testMethods = methods
                    .Select(method => new TestMethod(recorder, classIsDisposable, method))
                    .ToList();

                if (testMethods.Any())
                {
                    var testClass = new TestClass(this, @class, testMethods);

                    Exception? classLifecycleFailure = null;

                    try
                    {
                        await execution.RunAsync(testClass);
                    }
                    catch (Exception exception)
                    {
                        classLifecycleFailure = exception;
                    }

                    foreach (var testMethod in testMethods)
                    {
                        var testNeverRan = !testMethod.RecordedResult;

                        if (classLifecycleFailure != null)
                            await testMethod.FailAsync(classLifecycleFailure);
                        
                        if (testNeverRan)
                            await testMethod.SkipAsync("This test did not run.");
                    }
                }
            }
        }

        static bool IsDisposable(Type @class)
            => @class.GetInterfaces().Any(x => x == typeof(IAsyncDisposable) || x == typeof(IDisposable));
    }
}