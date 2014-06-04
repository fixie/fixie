using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fixie.Conventions
{
    public class Convention
    {
        public Convention()
        {
            Classes = new ClassFilter()
                .Where(type => !type.IsSubclassOf(typeof(Convention)) &&
                               !type.IsSubclassOf(typeof(TestAssembly)));
            Methods = new MethodFilter().Where(m => !m.IsDispose());
            CaseExecution = new CaseBehaviorBuilder();
            InstanceExecution = new InstanceBehaviorBuilder();
            ClassExecution = new ClassBehaviorBuilder();
            HideExceptionDetails = new AssertionLibraryFilter();

            MethodCallParameterBuilder = method => new object[][] { };
        }

        public ClassFilter Classes { get; private set; }
        public MethodFilter Methods { get; private set; }
        public CaseBehaviorBuilder CaseExecution { get; private set; }
        public InstanceBehaviorBuilder InstanceExecution { get; private set; }
        public ClassBehaviorBuilder ClassExecution { get; private set; }
        public AssertionLibraryFilter HideExceptionDetails { get; private set; }
        public Func<MethodInfo, IEnumerable<object[]>> MethodCallParameterBuilder { get; private set; }

        public void Parameters(Func<MethodInfo, IEnumerable<object[]>> getCaseParameters)
        {
            MethodCallParameterBuilder = getCaseParameters;
        }
    }
}