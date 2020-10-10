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
        readonly HashSet<string>? selectedTests;
        readonly Execution execution;

        internal TestAssembly(Assembly assembly, ExecutionRecorder recorder, IReadOnlyList<Type> classes,
            MethodDiscoverer methodDiscoverer, HashSet<string>? selectedTests, Execution execution)
        {
            Assembly = assembly;
            this.recorder = recorder;
            this.classes = classes;
            this.methodDiscoverer = methodDiscoverer;
            this.selectedTests = selectedTests;
            this.execution = execution;
        }

        public Assembly Assembly { get; }

        public void Run()
        {
            foreach (var @class in classes)
            {
                IEnumerable<MethodInfo> methods = methodDiscoverer.TestMethods(@class);

                if (selectedTests != null)
                    methods = methods.Where(method => selectedTests.Contains(new Test(method).Name));

                var testMethods = methods
                    .Select(method => new TestMethod(recorder, method))
                    .ToList();

                if (testMethods.Any())
                {
                    var targetMethod = classes.Count == 1 && testMethods.Count == 1
                        ? testMethods.Single()
                        : null;

                    var testClass = new TestClass(@class, testMethods, targetMethod?.Method);

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