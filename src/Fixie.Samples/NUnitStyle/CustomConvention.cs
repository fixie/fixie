using System;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Samples.NUnitStyle
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .HasOrInherits<TestFixtureAttribute>();

            Methods
                .HasOrInherits<TestAttribute>();

            ClassExecution
                    .CreateInstancePerClass()
                    .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));

            InstanceExecution
                .SetUpTearDown<TestFixtureSetUpAttribute, TestFixtureTearDownAttribute>();

            CaseExecution
                .SetUpTearDown<SetUpAttribute, TearDownAttribute>();
        }
    }

    public static class BehaviorBuilderExtensions
    {
        public static InstanceBehaviorBuilder SetUpTearDown<TSetUpAttribute, TTearDownAttribute>(this InstanceBehaviorBuilder builder)
            where TSetUpAttribute : Attribute
            where TTearDownAttribute : Attribute
        {
            return builder.SetUpTearDown(instanceExecution => InvokeAll<TSetUpAttribute>(instanceExecution.TestClass, instanceExecution.Instance),
                                         instanceExecution => InvokeAll<TTearDownAttribute>(instanceExecution.TestClass, instanceExecution.Instance));
        }

        public static CaseBehaviorBuilder SetUpTearDown<TSetUpAttribute, TTearDownAttribute>(this CaseBehaviorBuilder builder)
            where TSetUpAttribute : Attribute
            where TTearDownAttribute : Attribute
        {
            return builder.SetUpTearDown((caseExecution, instance) => InvokeAll<TSetUpAttribute>(caseExecution.Case.Class, instance),
                                         (caseExecution, instance) => InvokeAll<TTearDownAttribute>(caseExecution.Case.Class, instance));
        }

        static void InvokeAll<TAttribute>(Type type, object instance)
            where TAttribute : Attribute
        {
            foreach (var method in Has<TAttribute>().Filter(type))
            {
                try
                {
                    method.Invoke(instance, null);
                }
                catch (TargetInvocationException exception)
                {
                    throw new PreservedException(exception.InnerException);
                }
            }
        }

        static MethodFilter Has<TAttribute>() where TAttribute : Attribute
        {
            return new MethodFilter().HasOrInherits<TAttribute>();
        }
    }
}