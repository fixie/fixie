using System;
using Fixie.Behaviors;

namespace Fixie
{
    public class Fixture
    {
        public Fixture(Type testClass, object instance, CaseBehavior caseExecutionBehavior, InvokeBehavior invokeBehavior, Case[] cases)
        {
            TestClass = testClass;
            Instance = instance;
            CaseExecutionBehavior = caseExecutionBehavior;
            InvokeBehavior = invokeBehavior;
            Cases = cases;
        }

        public Type TestClass { get; private set; }
        public object Instance { get; private set; }
        public CaseBehavior CaseExecutionBehavior { get; private set; }
        public InvokeBehavior InvokeBehavior { get; private set; }
        public Case[] Cases { get; private set; }
    }
}