using System;
using System.Collections.Generic;
using Fixie.Conventions;

namespace Fixie
{
    public class TestClassInstance
    {
        public TestClassInstance(Convention convention, Type testClass, object instance, IReadOnlyList<CaseExecution> caseExecutions)
        {
            Convention = convention;
            TestClass = testClass;
            Instance = instance;
            CaseExecutions = caseExecutions;
        }

        public Convention Convention { get; private set; }
        public Type TestClass { get; private set; }
        public object Instance { get; private set; }
        public IReadOnlyList<CaseExecution> CaseExecutions { get; private set; }

        public void FailCases(Exception exception)
        {
            foreach (var caseExecution in CaseExecutions)
                caseExecution.Fail(exception);
        }
    }
}