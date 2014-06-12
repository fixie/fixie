using System;
using System.Collections.Generic;
using System.Linq;
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
                .Wrap<SupportExpectedExceptions>()
                .Wrap<SetUpTearDown>();
        }
    }

    class SupportExpectedExceptions : CaseBehavior
    {
        public void Execute(CaseExecution caseExecution, Action next)
        {
            next();

            var attribute = caseExecution.Case.Method.GetCustomAttributes<ExpectedExceptionAttribute>(false).SingleOrDefault();

            if (attribute == null)
                return;

            if (caseExecution.Exceptions.Count > 1)
                return;

            var exception = caseExecution.Exceptions.SingleOrDefault();

            if (exception == null)
                throw new Exception("Expected exception of type " + attribute.ExpectedException + ".");

            if (exception.GetType() != attribute.ExpectedException)
            {
                caseExecution.Pass();

                throw new Exception("Expected exception of type " + attribute.ExpectedException + " but an exception of type " + exception.GetType() + " was thrown.", exception);
            }

            if (attribute.ExpectedMessage != null && exception.Message != attribute.ExpectedMessage)
            {
                caseExecution.Pass();

                throw new Exception("Expected exception message '" + attribute.ExpectedMessage + "'" + " but was '" + exception.Message + "'.", exception);
            }

            caseExecution.Pass();
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
            foreach (var method in Has<TAttribute>(type))
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

        static IEnumerable<MethodInfo> Has<TAttribute>(Type type) where TAttribute : Attribute
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.HasOrInherits<TAttribute>());
        }
    }
}
