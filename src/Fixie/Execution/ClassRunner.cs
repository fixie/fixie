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
        readonly LifecycleRunner lifecycleRunner;
        readonly MethodDiscoverer methodDiscoverer;
        readonly ParameterDiscoverer parameterDiscoverer;
        readonly AssertionLibraryFilter assertionLibraryFilter;

        readonly IReadOnlyList<SkipBehavior> skipBehaviors;
        readonly Action<Case[]> orderCases;

        public ClassRunner(Bus bus, Filter filter, Convention convention)
        {
            var config = convention.Config;

            this.bus = bus;
            lifecycleRunner = new LifecycleRunner(convention.Config.Lifecycle);
            methodDiscoverer = new MethodDiscoverer(filter, convention);
            parameterDiscoverer = new ParameterDiscoverer(convention);
            assertionLibraryFilter = new AssertionLibraryFilter(convention);

            skipBehaviors = config.SkipBehaviors;
            orderCases = config.OrderCases;
        }

        public ExecutionSummary Run(Type testClass)
        {
            var methods = methodDiscoverer.TestMethods(testClass);

            var cases = new List<Case>();

            foreach (var method in methods)
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

            var orderedCases = cases.ToArray();
            try
            {
                orderCases(orderedCases);
            }
            catch (Exception exception)
            {
                // When an exception is thrown attempting to sort an array,
                // the behavior is undefined, so at this point orderedCases
                // is no longer reliable and needs to be fixed. The best we
                // can do is go with the original order.
                orderedCases = cases.ToArray();

                foreach (var @case in cases)
                    @case.Fail(exception);
            }

            var summary = new ExecutionSummary();

            if (!orderedCases.Any())
                return summary;

            Start(testClass);
            var stopwatch = Stopwatch.StartNew();

            var casesToExecute = new List<Case>();

            foreach (var @case in orderedCases)
            {
                if (@case.Exceptions.Any())
                    Fail(@case, summary);
                else
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
                        Fail(@case, summary);
                        continue;
                    }

                    if (skipCase)
                        Skip(@case, reason, summary);
                    else
                        casesToExecute.Add(@case);
                }
            }

            if (casesToExecute.Any())
            {
                Run(testClass, casesToExecute);

                foreach (var @case in casesToExecute)
                {
                    if (@case.Exceptions.Any())
                        Fail(@case, summary);
                    else
                        Pass(@case, summary);
                }
            }

            stopwatch.Stop();
            Complete(testClass, summary, stopwatch.Elapsed);

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

        void Run(Type testClass, IReadOnlyList<Case> casesToExecute)
        {
            lifecycleRunner.Execute(testClass, casesToExecute);
        }

        void Start(Type testClass)
        {
            bus.Publish(new ClassStarted(testClass));
        }

        void Skip(Case @case, string reason, ExecutionSummary summary)
        {
            var message = new CaseSkipped(@case, reason);
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