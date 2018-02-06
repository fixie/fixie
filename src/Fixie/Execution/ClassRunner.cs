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
        readonly AssertionLibraryFilter assertionLibraryFilter;

        readonly IReadOnlyList<SkipBehavior> skipBehaviors;
        readonly Action<MethodInfo[]> orderMethods;

        public ClassRunner(Bus bus, Filter filter, Convention convention)
        {
            var config = convention.Config;

            this.bus = bus;
            lifecycle = convention.Config.Lifecycle;
            methodDiscoverer = new MethodDiscoverer(filter, convention);
            parameterDiscoverer = new ParameterDiscoverer(convention);
            assertionLibraryFilter = new AssertionLibraryFilter(convention);

            skipBehaviors = config.SkipBehaviors;
            orderMethods = config.OrderMethods;
        }

        public ExecutionSummary Run(Type testClass)
        {
            var methods = methodDiscoverer.TestMethods(testClass);

            var summary = new ExecutionSummary();

            if (!methods.Any())
                return summary;

            Start(testClass);

            var classStopwatch = Stopwatch.StartNew();

            Exception orderException = null;

            var orderedMethods = methods.ToArray();
            try
            {
                if (orderedMethods.Length > 1)
                    orderMethods(orderedMethods);
            }
            catch (Exception exception)
            {
                // When an exception is thrown attempting to sort an array,
                // the behavior is undefined, so at this point orderedMethods
                // is no longer reliable and needs to be fixed. The best we
                // can do is go with the original order.
                orderedMethods = methods.ToArray();

                orderException = exception;
            }

            var cases = new List<Case>();

            foreach (var method in orderedMethods)
            {
                try
                {
                    bool generatedInputParameters = false;

                    foreach (var parameters in Parameters(method))
                    {
                        generatedInputParameters = true;
                        cases.Add(new Case(method, parameters));
                    }

                    if (!generatedInputParameters)
                    {
                        if (method.GetParameters().Length > 0)
                            throw new Exception("This test case has declared parameters, but no parameter values have been provided to it.");

                        cases.Add(new Case(method));
                    }
                }
                catch (Exception parameterGenerationException)
                {
                    var @case = new Case(method);
                    @case.Fail(parameterGenerationException);
                    cases.Add(@case);
                }
            }

            if (orderException != null)
                foreach (var @case in cases)
                    @case.Fail(orderException);

            var casesToExecute = new List<Case>();

            foreach (var @case in cases)
            {
                if (@case.Exception != null)
                    Fail(@case, summary);
                else
                    casesToExecute.Add(@case);
            }

            if (casesToExecute.Any())
            {
                try
                {
                    lifecycle.Execute(testClass, caseLifecycle =>
                    {
                        foreach (var @case in casesToExecute)
                        {
                            string reason;
                            bool skipCase;

                            try
                            {
                                skipCase = SkipCase(@case, out reason);
                            }
                            catch (Exception exception)
                            {
                                @case.Fail(exception);
                                continue;
                            }

                            if (skipCase)
                            {
                                @case.SkipReason = reason;
                                continue;
                            }

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
                                    Fail(new Case(@case, exception), summary);
                                }

                                caseStopwatch.Stop();

                                @case.Duration += caseStopwatch.Elapsed;

                                consoleOutput = console.Output;
                                @case.Output += consoleOutput;
                            }

                            Console.Write(consoleOutput);
                        }
                    });
                }
                catch (Exception exception)
                {
                    foreach (var @case in casesToExecute)
                        Fail(new Case(@case, exception), summary);
                }

                foreach (var @case in casesToExecute)
                {
                    if (@case.Exception != null)
                        Fail(@case, summary);
                    else if (@case.Executed)
                        Pass(@case, summary);
                    else
                        Skip(@case, summary);
                }
            }

            classStopwatch.Stop();
            Complete(testClass, summary, classStopwatch.Elapsed);

            return summary;
        }

        bool SkipCase(Case @case, out string reason)
        {
            var isTargetMethod = RunContext.TargetMethod == @case.Method;

            if (!isTargetMethod)
            {
                foreach (var skipBehavior in skipBehaviors)
                {
                    if (SkipCase(skipBehavior, @case))
                    {
                        reason = GetSkipReason(skipBehavior, @case);
                        return true;
                    }
                }
            }

            reason = null;
            return false;
        }

        bool SkipCase(SkipBehavior skipBehavior, Case @case)
        {
            try
            {
                return skipBehavior.SkipCase(@case);
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown while attempting to run a custom case-skipping predicate. " +
                    "Check the inner exception for more details.", exception);
            }
        }

        string GetSkipReason(SkipBehavior skipBehavior, Case @case)
        {
            try
            {
                return skipBehavior.GetSkipReason(@case);
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown while attempting to get a custom case-skipped reason. " +
                    "Check the inner exception for more details.", exception);
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
            var message = new CaseFailed(@case, assertionLibraryFilter);
            summary.Add(message);
            bus.Publish(message);
        }

        void Complete(Type testClass, ExecutionSummary summary, TimeSpan duration)
        {
            bus.Publish(new ClassCompleted(testClass, summary, duration));
        }
    }
}