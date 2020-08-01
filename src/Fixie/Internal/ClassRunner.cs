namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    class ClassRunner
    {
        static readonly object[] EmptyParameters = {};
        static readonly object[][] InvokeOnceWithZeroParameters = { EmptyParameters };

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
                    {
                        try
                        {
                            bool invoked = false;

                            var lazyInvocations = method.GetParameters().Length == 0
                                ? InvokeOnceWithZeroParameters
                                : parameterDiscoverer.GetParameters(method);

                            foreach (var parameters in lazyInvocations)
                            {
                                invoked = true;
                                Run(method, parameters, caseLifecycle, summary);
                            }

                            if (!invoked)
                                throw new Exception("This test has declared parameters, but no parameter values have been provided to it.");
                        }
                        catch (Exception exception)
                        {
                            Fail(method, exception, summary);
                        }
                    }
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
                    var @case = new Case(method, EmptyParameters);
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

        void Run(MethodInfo method, object?[] parameters, Action<Case> caseLifecycle, ExecutionSummary summary)
        {
            var @case = new Case(method, parameters);

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
            var @case = new Case(method, EmptyParameters);
            @case.Fail(exception);
            Fail(@case, summary);
        }

        void Complete(Type testClass, ExecutionSummary summary, TimeSpan duration)
        {
            bus.Publish(new ClassCompleted(testClass, summary, duration));
        }
    }
}