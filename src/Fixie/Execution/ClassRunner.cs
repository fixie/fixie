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

                    foreach (var @case in BuildCases(orderedMethods))
                    {
                        if (@case.Exception != null)
                        {
                            Fail(@case, summary);
                            continue;
                        }

                        string reason;
                        bool skipCase;

                        try
                        {
                            skipCase = SkipCase(@case, out reason);
                        }
                        catch (Exception exception)
                        {
                            @case.Fail(exception);
                            Fail(@case, summary);
                            continue;
                        }

                        if (skipCase)
                        {
                            @case.SkipReason = reason;
                            Skip(@case, summary);
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

                        if (@case.Exception != null)
                            Fail(@case, summary);
                        else if (@case.Executed)
                            Pass(@case, summary);
                        else
                            Skip(@case, summary);
                    }
                });
            }
            catch (Exception exception)
            {
                lifecycleThrew = true;
                foreach (var method in methods)
                {
                    var @case = new Case(method);
                    @case.Fail(exception);
                    Fail(@case, summary);
                }
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
                {
                    var @case = new Case(method);
                    @case.Fail(orderException);
                    Fail(@case, summary);
                }
            }

            return orderedMethods;
        }

        List<Case> BuildCases(MethodInfo[] orderedMethods)
        {
            var cases = new List<Case>();

            foreach (var method in orderedMethods)
            {
                try
                {
                    bool generatedInputParameters = false;

                    using (var resource = Parameters(method).GetEnumerator())
                    {
                        while (resource.MoveNext())
                        {
                            var parameters = resource.Current;

                            generatedInputParameters = true;
                            cases.Add(new Case(method, parameters));
                        }
                    }

                    if (!generatedInputParameters)
                    {
                        if (method.GetParameters().Length > 0)
                            throw new Exception(
                                "This test case has declared parameters, but no parameter values have been provided to it.");

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

            return cases;
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