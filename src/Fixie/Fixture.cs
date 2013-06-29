using System;
using Fixie.Behaviors;

namespace Fixie
{
    public class Fixture
    {
        public Fixture(Type testClass, object instance, MethodBehavior caseExecutionBehavior, Case[] cases)
        {
            TestClass = testClass;
            Instance = instance;
            CaseExecutionBehavior = caseExecutionBehavior;
            Cases = cases;
        }

        public Type TestClass { get; private set; }
        public object Instance { get; private set; }
        public MethodBehavior CaseExecutionBehavior { get; private set; }
        public Case[] Cases { get; private set; }
    }
}