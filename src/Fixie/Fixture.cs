using System;
using Fixie.Behaviors;

namespace Fixie
{
    public class Fixture
    {
        public Fixture(Type testClass, object instance, CaseBehavior caseExecutionBehavior, CaseExecution[] caseExecutions)
        {
            TestClass = testClass;
            Instance = instance;
            CaseExecutionBehavior = caseExecutionBehavior;
            CaseExecutions = caseExecutions;
        }

        public Type TestClass { get; private set; }
        public object Instance { get; private set; }
        public CaseBehavior CaseExecutionBehavior { get; private set; }
        public CaseExecution[] CaseExecutions { get; private set; }
    }
}