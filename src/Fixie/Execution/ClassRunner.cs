namespace Fixie.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    class ClassRunner
    {
        readonly Bus bus;
        readonly Lifecycle lifecycle;
        readonly MethodDiscoverer methodDiscoverer;
        readonly ParameterDiscoverer parameterDiscoverer;

        readonly Func<IReadOnlyList<MethodInfo>, IReadOnlyList<MethodInfo>> orderMethods;

        public ClassRunner(Bus bus, Filter filter, Convention convention)
        {
            var config = convention.Config;

            this.bus = bus;
            lifecycle = convention.Config.Lifecycle;
            methodDiscoverer = new MethodDiscoverer(filter, convention);
            parameterDiscoverer = new ParameterDiscoverer(convention);

            orderMethods = config.OrderMethods;
        }

        public ExecutionSummary Run(Type testClass, bool isOnlyTestClass)
        {
            var methods = methodDiscoverer.TestMethods(testClass);

            var summary = new ExecutionSummary();

            if (!methods.Any())
                return summary;

            var runContext = isOnlyTestClass && methods.Count == 1
                ? new TestClass(testClass, methods.Single())
                : new TestClass(testClass);

            Start(testClass);

            var classStopwatch = Stopwatch.StartNew();

            var orderedMethods = OrderedMethods(methods, summary);

            bool runCasesInvokedByLifecycle = false;
            lifecycle.Execute(runContext, caseLifecycle =>
            {
                if (runCasesInvokedByLifecycle)
                    throw new Exception($"{lifecycle.GetType()} attempted to run {testClass.FullName}'s test cases multiple times, which is not supported.");

                runCasesInvokedByLifecycle = true;

                foreach (var @case in YieldCases(orderedMethods, summary))
                {
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

                        if (consoleOutput.Length > 0)
                            @case.Output += consoleOutput;
                    }

                    Console.Write(consoleOutput);

                    var caseHasNormalResult = @case.State == CaseState.Failed || @case.State == CaseState.Passed;
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
            });

            if (!runCasesInvokedByLifecycle)
            {
                //No cases ran, and we didn't already emit a general
                //failure for each method, so emit a general skip for
                //each method.
                foreach (var method in orderedMethods)
                {
                    var @case = new Case(method);
                    Skip(@case, summary);
                }
            }

            classStopwatch.Stop();
            Complete(testClass, summary, classStopwatch.Elapsed);

            return summary;
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
                    Fail(method, orderException, summary);

                return methods;
            }
        }

        IEnumerable<Case> YieldCases(IReadOnlyList<MethodInfo> orderedMethods, ExecutionSummary summary)
        {
            foreach (var method in orderedMethods)
            {
                if (method.GetParameters().Length == 0)
                {
                    yield return new Case(method);
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

                            Fail(method, exception, summary);

                            break;
                        }

                        generatedInputParameters = true;
                        yield return new Case(method, parameters);
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
                    Fail(method, exception, summary);
                }
            }
        }

        IEnumerable<object[]> Parameters(MethodInfo method)
            => parameterDiscoverer.GetParameters(method);

        void Start(Type testClass)
        {
            bus.Publish(new ClassStarted(testClass));
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

        void Fail(MethodInfo method, Exception exception, ExecutionSummary summary)
        {
            var @case = new Case(method);
            @case.Fail(exception);
            Fail(@case, summary);
        }

        void Complete(Type testClass, ExecutionSummary summary, TimeSpan duration)
        {
            bus.Publish(new ClassCompleted(testClass, summary, duration));
        }
    }
}