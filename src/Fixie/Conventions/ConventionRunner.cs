using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Results;

namespace Fixie.Conventions
{
    public class ConventionRunner
    {
        public ConventionResult Run(Convention convention, Listener listener, params Type[] candidateTypes)
        {
            var conventionResult = new ConventionResult(convention.GetType().FullName);

            foreach (var testClass in convention.Classes.Filter(candidateTypes))
            {
                var classResult = new ClassResult(testClass.FullName);

                var methods = convention.Methods.Filter(testClass);

                var cases = methods.SelectMany(method => CasesForMethod(convention, method)).ToArray();
                var casesBySkipState = cases.ToLookup(convention.CaseExecution.SkipPredicate);
                var casesToSkip = casesBySkipState[true];
                var casesToExecute = casesBySkipState[false];
                foreach (var @case in casesToSkip)
                {
                    var skipResult = new SkipResult(@case, convention.CaseExecution.SkipReasonProvider(@case));
                    listener.CaseSkipped(skipResult);
                    classResult.Add(CaseResult.Skipped(skipResult.Case.Name, skipResult.Reason));
                }

                var caseExecutions = casesToExecute.Select(@case => new CaseExecution(@case)).ToArray();
                if (caseExecutions.Any())
                {
                    convention.ClassExecution.Behavior.Execute(new TestClass(convention, testClass), caseExecutions);

                    foreach (var caseExecution in caseExecutions)
                    {
                        if (caseExecution.Exceptions.Any())
                        {
                            var failResult = new FailResult(caseExecution, convention.HideExceptionDetails);
                            listener.CaseFailed(failResult);
                            classResult.Add(CaseResult.Failed(failResult.Case.Name, failResult.Duration, failResult.ExceptionSummary));
                        }
                        else
                        {
                            var passResult = new PassResult(caseExecution);
                            listener.CasePassed(passResult);
                            classResult.Add(CaseResult.Passed(passResult.Case.Name, passResult.Duration));
                        }
                    }

                }

                conventionResult.Add(classResult);
            }

            return conventionResult;
        }

        static IEnumerable<Case> CasesForMethod(Convention convention, MethodInfo method)
        {
            var casesForKnownInputParameters = convention.MethodCallParameterBuilder(method)
                .Select(parameters => new Case(method, parameters));

            bool any = false;

            foreach (var actualCase in casesForKnownInputParameters)
            {
                any = true;
                yield return actualCase;
            }

            if (!any)
            {
                if (method.GetParameters().Any())
                    yield return new UncallableParameterizedCase(method);
                else
                    yield return new Case(method);
            }
        }
    }
}