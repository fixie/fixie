namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class TestAssembly
    {
        readonly Assembly assembly;
        readonly string[] customArguments;
        readonly Bus bus;

        public TestAssembly(Assembly assembly, Listener listener)
            : this(assembly, new string[] {}, listener) { }

        public TestAssembly(Assembly assembly, string[] customArguments, params Listener[] listeners)
        {
            this.assembly = assembly;
            this.customArguments = customArguments;
            bus = new Bus(listeners);
        }

        public void Discover()
        {
            var discovery = new BehaviorDiscoverer(assembly, customArguments).GetDiscovery();

            try
            {
                Discover(assembly.GetTypes(), discovery);
            }
            finally
            {
                discovery.Dispose();
            }
        }

        public ExecutionSummary Run()
        {
            return Run(assembly.GetTypes());
        }

        public ExecutionSummary Run(IReadOnlyList<Test> tests)
        {
            var request = new Dictionary<string, HashSet<string>>();
            var types = new List<Type>();

            foreach (var test in tests)
            {
                if (!request.ContainsKey(test.Class))
                {
                    request.Add(test.Class, new HashSet<string>());

                    var type = assembly.GetType(test.Class);

                    if (type != null)
                        types.Add(type);
                }

                request[test.Class].Add(test.Method);
            }

            return Run(types, method => request[method.ReflectedType!.FullName!].Contains(method.Name));
        }

        ExecutionSummary Run(IReadOnlyList<Type> candidateTypes, Func<MethodInfo, bool>? selected = null)
        {
            new BehaviorDiscoverer(assembly, customArguments)
                .GetBehaviors(out var discovery, out var execution);

            try
            {
                return Run(candidateTypes, discovery, execution, selected);
            }
            finally
            {
                if (execution != discovery)
                    execution.Dispose();

                discovery.Dispose();
            }
        }

        internal void Discover(IReadOnlyList<Type> candidateTypes, Discovery discovery)
        {
            var classDiscoverer = new ClassDiscoverer(discovery);
            var testClasses = classDiscoverer.TestClasses(candidateTypes);

            var methodDiscoverer = new MethodDiscoverer(discovery);
            foreach (var testClass in testClasses)
            foreach (var testMethod in methodDiscoverer.TestMethods(testClass))
                bus.Publish(new TestDiscovered(new Test(testMethod)));
        }

        internal ExecutionSummary Run(IReadOnlyList<Type> candidateTypes, Discovery discovery, Execution execution, Func<MethodInfo, bool>? selected = null)
        {
            var recorder = new ExecutionRecorder(bus);
            
            recorder.Start(assembly);
            
            var classDiscoverer = new ClassDiscoverer(discovery);
            var methodDiscoverer = new MethodDiscoverer(discovery);

            var classes = classDiscoverer.TestClasses(candidateTypes);

            Run(recorder, classes, methodDiscoverer, selected, execution);

            return recorder.Complete(assembly);
        }

        static void Run(ExecutionRecorder recorder, IReadOnlyList<Type> classes, MethodDiscoverer methodDiscoverer, Func<MethodInfo, bool>? selected, Execution execution)
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