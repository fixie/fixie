using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie
{
    public class Convention
    {
        public Convention()
        {
            Config = new Configuration();

            Classes = new TestClassExpression(Config);
            Methods = new TestMethodExpression(Config);;
            Parameters = new ParameterSourceExpression(Config);
            CaseExecution = new CaseBehaviorExpression(Config);
            FixtureExecution = new FixtureBehaviorExpression(Config);
            ClassExecution = new ClassBehaviorExpression(Config);
            HideExceptionDetails = new AssertionLibraryExpression(Config);
        }

        public Configuration Config { get; private set; }

        public TestClassExpression Classes { get; private set; }
        public TestMethodExpression Methods { get; private set; }
        public ParameterSourceExpression Parameters { get; private set; }

        public CaseBehaviorExpression CaseExecution { get; private set; }
        public FixtureBehaviorExpression FixtureExecution { get; private set; }
        public ClassBehaviorExpression ClassExecution { get; private set; }
        public AssertionLibraryExpression HideExceptionDetails { get; private set; }
    }
}