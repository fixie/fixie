using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Execution;

namespace Fixie.Internal
{
    public class ClassRunner
    {
        readonly Bus bus;
        readonly ExecutionPlan executionPlan;
        readonly MethodDiscoverer methodDiscoverer;
        readonly ParameterDiscoverer parameterDiscoverer;
        readonly AssertionLibraryFilter assertionLibraryFilter;

        readonly IReadOnlyList<SkipBehavior> skipBehaviors;
        readonly Action<Case[]> orderCases;

        public ClassRunner(Bus bus, Convention convention)
        {
            var config = convention.Config;

            this.bus = bus;
            executionPlan = new ExecutionPlan(convention);
            methodDiscoverer = new MethodDiscoverer(convention);
            parameterDiscoverer = new ParameterDiscoverer(convention);
            assertionLibraryFilter = new AssertionLibraryFilter(convention);

            skipBehaviors = config.SkipBehaviors;
            orderCases = config.OrderCases;
        }

        public ClassResult Run(Type testClass)
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

            var classResult = new ClassResult(testClass.FullName);

            var casesToExecute = new List<Case>();

            foreach (var @case in orderedCases)
            {
                if (@case.Exceptions.Any())
                    classResult.Add(Fail(@case));
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
                        classResult.Add(Fail(@case));
                        continue;
                    }

                    if (skipCase)
                        classResult.Add(Skip(@case, reason));
                    else
                        casesToExecute.Add(@case);
                }
            }

            if (casesToExecute.Any())
            {
                Run(testClass, casesToExecute);

                foreach (var @case in casesToExecute)
                    classResult.Add(@case.Exceptions.Any() ? Fail(@case) : Pass(@case));
            }

            return classResult;
        }

        bool SkipCase(Case @case, out string reason)
        {
            foreach (var skipBehavior in skipBehaviors)
            {
                if (SkipCase(skipBehavior, @case))
                {
                    reason = GetSkipReason(skipBehavior, @case);
                    return true;
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
        {
            return parameterDiscoverer.GetParameters(method);
        }

        void Run(Type testClass, IReadOnlyList<Case> casesToExecute)
            => executionPlan.ExecuteClassBehaviors(new Class(testClass, casesToExecute));

        CaseResult Skip(Case @case, string reason)
        {
            var result = new CaseSkipped(@case, reason);
            bus.Publish(result);
            return result;
        }

        CaseResult Pass(Case @case)
        {
            var result = new CasePassed(@case);
            bus.Publish(result);
            return result;
        }

        CaseResult Fail(Case @case)
        {
            var result = new CaseFailed(@case, assertionLibraryFilter);
            bus.Publish(result);
            return result;
        }
    }
}