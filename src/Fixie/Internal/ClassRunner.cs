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
        readonly Stopwatch caseStopwatch;

        readonly Func<IReadOnlyList<MethodInfo>, IReadOnlyList<MethodInfo>> orderMethods;

        public ClassRunner(Bus bus, Discovery discovery, Execution execution)
        {
            var config = discovery.Config;

            this.bus = bus;
            this.execution = execution;
            methodDiscoverer = new MethodDiscoverer(discovery);
            parameterDiscoverer = new ParameterDiscoverer(discovery);
            caseStopwatch = new Stopwatch();

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
            caseStopwatch.Restart();

            var orderedMethods = OrderedMethods(methods, summary);

            bool classLifecycleFailed = false;
            bool runCasesInvokedByLifecycle = false;

            try
            {
                Action<Action<Case>> runCases = caseLifecycle =>
                {
                    runCasesInvokedByLifecycle = true;

                    foreach (var method in orderedMethods)
                        foreach (var @case in YieldCases(method, summary))
                            Run(@case, caseLifecycle, summary);
                };

                var runContext = isOnlyTestClass && methods.Count == 1
                    ? new TestClass(testClass, runCases, methods.Single())
                    : new TestClass(testClass, runCases);

                execution.Execute(runContext);
            }
            catch (Exception exception)
            {
                classLifecycleFailed = true;
                foreach (var method in orderedMethods)
                    Fail(method, exception, summary);
            }

            if (!runCasesInvokedByLifecycle && !classLifecycleFailed)
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

        IEnumerable<Case> YieldCases(MethodInfo method, ExecutionSummary summary)
        {
            if (method.GetParameters().Length == 0)
            {
                yield return new Case(method);
                yield break;
            }

            bool generatedInputParameters = false;
            bool parameterGenerationThrew = false;

            using (var resource = Parameters(method).GetEnumerator())
            {
                while (true)
                {
                    object?[] parameters;

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
                yield break;

            try
            {
                throw new Exception(
                    "This test case has declared parameters, but no parameter values have been provided to it.");
            }
            catch (Exception exception)
            {
                Fail(method, exception, summary);
            }
        }

        void Run(Case @case, Action<Case> caseLifecycle, ExecutionSummary summary)
        {
            Start(@case);

            Exception? caseLifecycleFailure = null;

            string output;
            using (var console = new RedirectedConsole())
            {
                try
                {
                    caseLifecycle(@case);
                }
                catch (Exception exception)
                {
                    caseLifecycleFailure = exception;
                }

                output = console.Output;
            }

            Console.Write(output);

            var caseHasNormalResult = @case.State == CaseState.Failed || @case.State == CaseState.Passed;

            if (caseHasNormalResult)
            {
                if (@case.State == CaseState.Failed)
                    Fail(@case, summary, output);
                else if (caseLifecycleFailure == null)
                    Pass(@case, summary, output);
            }

            if (caseLifecycleFailure != null)
                Fail(new Case(@case, caseLifecycleFailure), summary);
            else if (!caseHasNormalResult)
                Skip(@case, summary, output);
        }

        IEnumerable<object?[]> Parameters(MethodInfo method)
            => parameterDiscoverer.GetParameters(method);

        void Start(Type testClass)
        {
            bus.Publish(new ClassStarted(testClass));
        }

        void Start(Case @case)
        {
            bus.Publish(new CaseStarted(@case));
        }

        void Skip(Case @case, ExecutionSummary summary, string output = "")
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CaseSkipped(@case, duration, output);
            summary.Add(message);
            bus.Publish(message);
        }

        void Pass(Case @case, ExecutionSummary summary, string output)
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CasePassed(@case, duration, output);
            summary.Add(message);
            bus.Publish(message);
        }

        void Fail(Case @case, ExecutionSummary summary, string output = "")
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CaseFailed(@case, duration, output);
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