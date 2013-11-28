using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Conventions
{
    public class Convention
    {
        Func<MethodInfo, IEnumerable<object[]>> methodCallParameterBuilder;

        public Convention()
        {
            Classes = new ClassFilter().Where(type => !type.IsSubclassOf(typeof(Convention)));
            Methods = new MethodFilter().Where(m => !m.IsDispose());
            CaseExecution = new CaseBehaviorBuilder();
            InstanceExecution = new InstanceBehaviorBuilder();
            ClassExecution = new TypeBehaviorBuilder().CreateInstancePerCase();

            methodCallParameterBuilder = method => new object[][] { };
        }

        public ClassFilter Classes { get; private set; }
        public MethodFilter Methods { get; private set; }
        public CaseBehaviorBuilder CaseExecution { get; private set; }
        public InstanceBehaviorBuilder InstanceExecution { get; private set; }
        public TypeBehaviorBuilder ClassExecution { get; private set; }

        public void Parameters(Func<MethodInfo, IEnumerable<object[]>> getCaseParameters)
        {
            methodCallParameterBuilder = getCaseParameters;
        }

        public void Execute(Listener listener, params Type[] candidateTypes)
        {
            foreach (var testClass in Classes.Filter(candidateTypes))
            {
                var methods = Methods.Filter(testClass);

                var cases = methods.SelectMany(method => CasesForMethod(testClass, method)).ToArray();
                var casesBySkipState = cases.ToLookup(CaseExecution.SkipPredicate);
                var casesToSkip = casesBySkipState[true];
                var casesToExecute = casesBySkipState[false];
                foreach (var @case in casesToSkip)
                {
                    //listener.CaseSkipped(new SkipResult(@case));
                }

                var caseExecutions = casesToExecute.Select(@case => new CaseExecution(@case)).ToArray();

                ClassExecution.Behavior.Execute(testClass, this, caseExecutions);

                foreach (var caseExecution in caseExecutions)
                {
                    if (caseExecution.Result == CaseResult.Failed)
                        listener.CaseFailed(new FailResult(caseExecution));
                    else
                        listener.CasePassed(new PassResult(caseExecution));
                }
            }
        }

        IEnumerable<Case> CasesForMethod(Type testClass, MethodInfo method)
        {
            var casesForKnownInputParameters = methodCallParameterBuilder(method)
                .Select(parameters => new Case(testClass, method, parameters));

            bool any = false;

            foreach (var actualCase in casesForKnownInputParameters)
            {
                any = true;
                yield return actualCase;
            }

            if (!any)
            {
                if (method.GetParameters().Any())
                    yield return new UncallableParameterizedCase(testClass, method);
                else
                    yield return new Case(testClass, method);
            }
        }
    }
}