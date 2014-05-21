using System;
using System.Reflection;
using Fixie.Behaviors;
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
                .Wrap<FixtureSetUpTearDown>();

            CaseExecution
                .Wrap<SetUpTearDown>();
        }
    }

    class SetUpTearDown : CaseBehavior
    {
        public void Execute(CaseExecution caseExecution, Action next)
        {
            caseExecution.Case.Class.InvokeAll<SetUpAttribute>(caseExecution.Instance);
            next();
            caseExecution.Case.Class.InvokeAll<TearDownAttribute>(caseExecution.Instance);
        }
    }

    class FixtureSetUpTearDown : InstanceBehavior
    {
        public void Execute(InstanceExecution instanceExecution, Action next)
        {
            instanceExecution.TestClass.InvokeAll<TestFixtureSetUpAttribute>(instanceExecution.Instance);
            next();
            instanceExecution.TestClass.InvokeAll<TestFixtureTearDownAttribute>(instanceExecution.Instance);
        }
    }

    public static class BehaviorBuilderExtensions
    {
        public static void InvokeAll<TAttribute>(this Type type, object instance)
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