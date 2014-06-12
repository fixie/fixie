using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.DSL;

namespace Fixie.Conventions
{
    public class Convention
    {
        public Convention()
        {
            Config = new ConfigModel();

            Classes = new ClassFilter()
                .Where(type => !type.IsSubclassOf(typeof(Convention)) &&
                               !type.IsSubclassOf(typeof(TestAssembly)));
            Methods = new MethodFilter().Where(m => !m.IsDispose());
            CaseExecution = new CaseBehaviorExpression(Config);
            InstanceExecution = new InstanceBehaviorExpression(Config);
            ClassExecution = new ClassBehaviorExpression(Config);
            HideExceptionDetails = new AssertionLibraryExpression(Config);
        }

        public ConfigModel Config { get; private set; }

        public ClassFilter Classes { get; private set; }
        public MethodFilter Methods { get; private set; }
        public CaseBehaviorExpression CaseExecution { get; private set; }
        public InstanceBehaviorExpression InstanceExecution { get; private set; }
        public ClassBehaviorExpression ClassExecution { get; private set; }
        public AssertionLibraryExpression HideExceptionDetails { get; private set; }

        public void Parameters(Func<MethodInfo, IEnumerable<object[]>> getCaseParameters)
        {
            Config.GetCaseParameters = getCaseParameters;
        }
    }
}