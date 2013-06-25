using System;
using Fixie.Behaviors;

namespace Fixie
{
    public class Fixture
    {
        public Fixture(Type type, object instance, MethodBehavior caseExecutionBehavior, Case[] cases)
        {
            Type = type;
            Instance = instance;
            CaseExecutionBehavior = caseExecutionBehavior;
            Cases = cases;
        }

        public Type Type { get; private set; }
        public object Instance { get; private set; }
        public MethodBehavior CaseExecutionBehavior { get; private set; }
        public Case[] Cases { get; private set; }
    }
}