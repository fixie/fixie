using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        public void Execute(Case @case, Action next)
        {
            next();

            var attribute = @case.Method.GetCustomAttributes<ExpectedExceptionAttribute>(false).SingleOrDefault();

            if (attribute == null)
                return;

            if (@case.Exceptions.Count > 1)
                return;

            var exception = @case.Exceptions.SingleOrDefault();

            if (exception == null)
                throw new Exception("Expected exception of type " + attribute.ExpectedException + ".");

            if (exception.GetType() != attribute.ExpectedException)
            {
                @case.ClearExceptions();

                throw new Exception("Expected exception of type " + attribute.ExpectedException + " but an exception of type " + exception.GetType() + " was thrown.", exception);
            }

            if (attribute.ExpectedMessage != null && exception.Message != attribute.ExpectedMessage)
            {
                @case.ClearExceptions();

                throw new Exception("Expected exception message '" + attribute.ExpectedMessage + "'" + " but was '" + exception.Message + "'.", exception);
            }

            @case.ClearExceptions();
        }
    }

    class SetUpTearDown : CaseBehavior
    {
        public void Execute(Case @case, Action next)
        {
            @case.Class.InvokeAll<SetUpAttribute>(@case.Fixture.Instance);
            next();
            @case.Class.InvokeAll<TearDownAttribute>(@case.Fixture.Instance);
        }
    }

    class FixtureSetUpTearDown : FixtureBehavior
    {
        public void Execute(Fixture fixture, Action next)
        {
            fixture.TestClass.InvokeAll<TestFixtureSetUpAttribute>(fixture.Instance);
            next();
            fixture.TestClass.InvokeAll<TestFixtureTearDownAttribute>(fixture.Instance);
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
