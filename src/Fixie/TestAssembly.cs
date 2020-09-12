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
        readonly Func<MethodInfo, bool>? selected;
        readonly Execution execution;

        internal TestAssembly(Assembly assembly, ExecutionRecorder recorder, IReadOnlyList<Type> classes,
            MethodDiscoverer methodDiscoverer, Func<MethodInfo, bool>? selected, Execution execution)
        {
            Assembly = assembly;
            this.recorder = recorder;
            this.classes = classes;
            this.methodDiscoverer = methodDiscoverer;
            this.selected = selected;
            this.execution = execution;
        }

        public Assembly Assembly { get; }

        public void Run()
        {
            foreach (var @class in classes)
            {
                IEnumerable<MethodInfo> methods = methodDiscoverer.TestMethods(@class);

                if (selected != null)
                    methods = methods.Where(selected);

                var testMethods = methods
                    .Select(method => new TestMethod(recorder, method))
                    .ToList();

                if (testMethods.Any())
                {
                    var targetMethod = classes.Count == 1 && testMethods.Count == 1
                        ? testMethods.Single()
                        : null;

                    var testClass = new TestClass(@class, testMethods, targetMethod?.Method);

                    recorder.Start(testClass);

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
                        if (!testMethod.RecordedResult)
                            testMethod.Skip("This test did not run.");

                        if (classLifecycleFailure != null)
                            testMethod.Fail(classLifecycleFailure);
                    }

                    recorder.Complete(testClass);
                }
            }
        }
    }
}