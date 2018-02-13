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

            var orderedMethods = OrderedMethods(methods, summary);

            bool lifecycleThrew = false;
            bool runCasesInvokedByLifecycle = false;

            try
            {
                lifecycle.Execute(testClass, caseLifecycle =>
                {
                    if (runCasesInvokedByLifecycle)
                        throw new Exception($"{lifecycle.GetType()} attempted to run {testClass.FullName}'s test cases multiple times, which is not supported.");

                    runCasesInvokedByLifecycle = true;

                    foreach (var @case in YieldCases(orderedMethods, summary))
                    {
                        try
                        {
                            var skipCase = SkipCase(@case, out var reason);

                            if (skipCase)
                            {
                                @case.SkipReason = reason;
                                Skip(@case, summary);
                                continue;
                            }
                        }
                        catch (Exception exception)
                        {
                            @case.Fail(exception);
                            Fail(@case, summary);
                            continue;
                        }

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

                        var caseHasNormalResult = @case.Exception != null || @case.Executed;
                        var caseLifecycleFailed = caseLifecycleException != null;

                        if (caseHasNormalResult)
                        {
                            if (@case.Exception != null)
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
            }
            catch (Exception exception)
            {
                lifecycleThrew = true;
                foreach (var method in methods)
                    Fail(method, exception, summary);
            }

            if (!runCasesInvokedByLifecycle && !lifecycleThrew)
            {
                //No cases ran, and we didn't already emit a general
                //failure for each method, so emit a general skip for
                //each method.
                foreach (var method in methods)
                {
                    var @case = new Case(method);
                    Skip(@case, summary);
                }
            }

            classStopwatch.Stop();
            Complete(testClass, summary, classStopwatch.Elapsed);

            return summary;
        }

        MethodInfo[] OrderedMethods(IReadOnlyList<MethodInfo> methods, ExecutionSummary summary)
        {
            var orderedMethods = methods.ToArray();

            try
            {
                if (orderedMethods.Length > 1)
                    orderMethods(orderedMethods);
            }
            catch (Exception orderException)
            {
                // When an exception is thrown attempting to sort an array,
                // the behavior is undefined, so at this point orderedMethods
                // is no longer reliable and needs to be fixed. The best we
                // can do is go with the original order.
                orderedMethods = methods.ToArray();

                foreach (var method in methods)
                    Fail(method, orderException, summary);
            }

            return orderedMethods;
        }

        IEnumerable<Case> YieldCases(MethodInfo[] orderedMethods, ExecutionSummary summary)
        {
            foreach (var method in orderedMethods)
            {
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

                if (method.GetParameters().Length > 0)
                {
                    try
                    {
                        throw new Exception("This test case has declared parameters, but no parameter values have been provided to it.");
                    }
                    catch (Exception exception)
                    {
                        Fail(method, exception, summary);
                    }
                }
                else
                {
                    yield return new Case(method);
                }
            }
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