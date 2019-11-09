namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    class ClassRunner
    {
        readonly Bus bus;
        readonly Execution execution;
        readonly MethodDiscoverer methodDiscoverer;
        readonly ParameterDiscoverer parameterDiscoverer;

        readonly Func<IReadOnlyList<MethodInfo>, IReadOnlyList<MethodInfo>> orderMethods;

        public ClassRunner(Bus bus, Discovery discovery, Execution execution)
        {
            var config = discovery.Config;

            this.bus = bus;
            this.execution = execution;
            methodDiscoverer = new MethodDiscoverer(discovery);
            parameterDiscoverer = new ParameterDiscoverer(discovery);

            orderMethods = config.OrderMethods;
        }

        public ExecutionSummary Run(Type testClass, bool isOnlyTestClass)
        {
            var methods = methodDiscoverer.TestMethods(testClass);

            var summary = new ExecutionSummary();

            if (!methods.Any())
                return summary;

            Start(testClass);

            var classStopwatch = Stopwatch.StartNew();

            var orderedMethods = OrderedMethods(methods, summary);

            bool classLifecycleFailed = false;
            bool runCasesInvokedByLifecycle = false;

            var testClassConstructors = testClass.GetConstructors().ToArray();
            foreach (var constructorInfo in testClassConstructors)
            {
                var dataSets = parameterDiscoverer.GetParameters(constructorInfo).ToArray();
                if (!dataSets.Any())
                {
                    dataSets = new []
                    {
                        new object[0]
                    };
                }

                foreach (var constructorParameters in dataSets)
                {
                    runCasesInvokedByLifecycle = RunCasesInvokedByLifecycle(constructorInfo, constructorParameters);
                }
            }

            if (!testClassConstructors.Any())
            {
                runCasesInvokedByLifecycle = RunCasesInvokedByLifecycle(null, new object[0][]);
            }

            if (!runCasesInvokedByLifecycle && !classLifecycleFailed)
            {
                //No cases ran, and we didn't already emit a general
                //failure for each method, so emit a general skip for
                //each method.
                foreach (var method in orderedMethods)
                {
                    var @case = new Case(method, null, null);
                    Skip(@case, summary);
                }
            }

            classStopwatch.Stop();
            Complete(testClass, summary, classStopwatch.Elapsed);

            return summary;

            bool RunCasesInvokedByLifecycle(ConstructorInfo constructorInfo, object[] constructorParameters)
            {
                try
                {
                    Action<Action<Case>> runCases = caseLifecycle =>
                    {
                        runCasesInvokedByLifecycle = true;

                        foreach (var @case in YieldCases(orderedMethods, summary, constructorParameters))
                        {
                            Start(@case);

                            Exception caseLifecycleException = null;

                            string consoleOutput;
                            using (var console = new RedirectedConsole())
                            {
                                var caseStopwatch = Stopwatch.StartNew();

                                try
                                {
                                    caseLifecycle(@case);
                                }
                                catch (Exception exception)
                                {
                                    caseLifecycleException = exception;
                                }

                                caseStopwatch.Stop();

                                @case.Duration += caseStopwatch.Elapsed;

                                consoleOutput = console.Output;

                                @case.Output += consoleOutput;
                            }

                            Console.Write(consoleOutput);

                            var caseHasNormalResult =
                                @case.State == CaseState.Failed || @case.State == CaseState.Passed;
                            var caseLifecycleFailed = caseLifecycleException != null;

                            if (caseHasNormalResult)
                            {
                                if (@case.State == CaseState.Failed)
                                    Fail(@case, summary);
                                else if (!caseLifecycleFailed)
                                    Pass(@case, summary);
                            }

                            if (caseLifecycleFailed)
                                Fail(new Case(@case, caseLifecycleException), summary);
                            else if (!caseHasNormalResult)
                                Skip(@case, summary);
                        }
                    };

                    var runContext = isOnlyTestClass && methods.Count == 1
                        ? new TestClass(testClass, runCases, constructorInfo, constructorParameters, methods.Single())
                        : new TestClass(testClass, runCases, constructorInfo, constructorParameters);

                    execution.Execute(runContext);
                }
                catch (Exception exception)
                {
                    classLifecycleFailed = true;
                    foreach (var method in orderedMethods)
                        Fail(method, exception, summary, constructorParameters);
                }

                return runCasesInvokedByLifecycle;
            }
        }

        IReadOnlyList<MethodInfo> OrderedMethods(IReadOnlyList<MethodInfo> methods, ExecutionSummary summary)
        {
            try
            {
                return orderMethods(methods);
            }
            catch (Exception orderException)
            {
                foreach (var method in methods)
                    Fail(method, orderException, summary, null);

                return methods;
            }
        }

        IEnumerable<Case> YieldCases(IReadOnlyList<MethodInfo> orderedMethods, ExecutionSummary summary, object[] ctorParameters)
        {
            foreach (var method in orderedMethods)
            {
                if (method.GetParameters().Length == 0)
                {
                    yield return new Case(method, new object[0], ctorParameters);
                    continue;
                }

                bool generatedInputParameters = false;
                bool parameterGenerationThrew = false;

                using (var resource = Parameters(method).GetEnumerator())
                {
                    while (true)
                    {
                        object[] parameters;

                        try
                        {
                            if (!resource.MoveNext())
                                break;

                            parameters = resource.Current;
                        }
                        catch (Exception exception)
                        {
                            parameterGenerationThrew = true;

                            Fail(method, exception, summary, ctorParameters);

                            break;
                        }

                        generatedInputParameters = true;
                        yield return new Case(method, parameters, ctorParameters);
                    }
                }

                if (parameterGenerationThrew || generatedInputParameters)
                    continue;

                try
                {
                    throw new Exception("This test case has declared parameters, but no parameter values have been provided to it.");
                }
                catch (Exception exception)
                {
                    Fail(method, exception, summary, ctorParameters);
                }
            }
        }

        IEnumerable<object[]> Parameters(MethodInfo method)
            => parameterDiscoverer.GetParameters(method);

        void Start(Type testClass)
        {
            bus.Publish(new ClassStarted(testClass));
        }

        void Start(Case @case)
        {
            bus.Publish(new CaseStarted(@case));
        }

        void Skip(Case @case, ExecutionSummary summary)
        {
            var message = new CaseSkipped(@case);
            summary.Add(message);
            bus.Publish(message);
        }

        void Pass(Case @case, ExecutionSummary summary)
        {
            var message = new CasePassed(@case);
            summary.Add(message);
            bus.Publish(message);
        }

        void Fail(Case @case, ExecutionSummary summary)
        {
            var message = new CaseFailed(@case);
            summary.Add(message);
            bus.Publish(message);
        }

        void Fail(MethodInfo method, Exception exception, ExecutionSummary summary, object[] ctorParameters)
        {
            var @case = new Case(method, new object[0], ctorParameters);
            @case.Fail(exception);
            Fail(@case, summary);
        }

        void Complete(Type testClass, ExecutionSummary summary, TimeSpan duration)
        {
            bus.Publish(new ClassCompleted(testClass, summary, duration));
        }
    }
}