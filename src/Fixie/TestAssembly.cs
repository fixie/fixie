namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Internal;

    public class TestAssembly
    {
        readonly ExecutionRecorder recorder;
        readonly IReadOnlyList<Type> classes;
        readonly MethodDiscoverer methodDiscoverer;
        readonly Execution execution;

        internal TestAssembly(Assembly assembly, HashSet<string>? selectedTests, ExecutionRecorder recorder,
            IReadOnlyList<Type> classes,
            MethodDiscoverer methodDiscoverer, Execution execution)
        {
            Assembly = assembly;
            SelectedTests = selectedTests;

            this.recorder = recorder;
            this.classes = classes;
            this.methodDiscoverer = methodDiscoverer;
            this.execution = execution;
        }

        internal Assembly Assembly { get; }

        /// <summary>
        /// Gets the set of explicitly selected test names to be executed.
        /// Null under normal test execution when all tests are being executed.
        /// </summary>
        public HashSet<string>? SelectedTests { get; }

        internal void Run()
        {
            foreach (var @class in classes)
            {
                IEnumerable<MethodInfo> methods = methodDiscoverer.TestMethods(@class);

                if (SelectedTests != null)
                    methods = methods.Where(method => SelectedTests.Contains(new Test(method).Name));

                var testMethods = methods
                    .Select(method => new TestMethod(recorder, method))
                    .ToList();

                if (testMethods.Any())
                {
                    var testClass = new TestClass(this, @class, testMethods);

                    Exception? classLifecycleFailure = null;

                    try
                    {
                        execution.Execute(testClass);
                    }
                    catch (Exception exception)
                    {
                        classLifecycleFailure = exception;
                    }

                    foreach (var testMethod in testMethods)
                    {
                        var testNeverRan = !testMethod.RecordedResult;

                        if (classLifecycleFailure != null)
                            testMethod.Fail(classLifecycleFailure);
                        
                        if (testNeverRan)
                            testMethod.Skip("This test did not run.");
                    }
                }
            }
        }
    }
}