namespace Fixie.Samples.NUnitStyle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .HasOrInherits<TestFixtureAttribute>();

            Methods
                .HasOrInherits<TestAttribute>();

            Parameters
                .Add<TestCaseSourceAttributeParameterSource>();

            ClassExecution
                    .CreateInstancePerClass()
                    .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));

            FixtureExecution
                .Wrap<FixtureSetUpTearDown>();

            CaseExecution
                .Wrap<SupportExpectedExceptions>()
                .Wrap<SetUpTearDown>();
        }
    }

    class TestCaseSourceAttributeParameterSource : ParameterSource
    {
        public IEnumerable<object[]> GetParameters(MethodInfo method)
        {
            var testInvocations = new List<object[]>();

            var testCaseSourceAttributes = method.GetCustomAttributes<TestCaseSourceAttribute>(true).ToList();

            foreach (var attribute in testCaseSourceAttributes)
            {
                var sourceType = attribute.SourceType ?? method.ReflectedType;

                if (sourceType == null)
                    throw new Exception("Could not find source type for method " + method.Name);

                var members = sourceType.GetMember(attribute.SourceName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                if (members.Length != 1)
                    throw new Exception($"Found {members.Length} members named '{attribute.SourceName}' on type {sourceType}");

                var member = members.Single();

                testInvocations.AddRange(InvocationsForTestCaseSource(member));
            }

            return testInvocations;
        }

        private static IEnumerable<object[]> InvocationsForTestCaseSource(MemberInfo member)
        {
            var field = member as FieldInfo;
            if (field != null && field.IsStatic)
                return (IEnumerable<object[]>)field.GetValue(null);

            var property = member as PropertyInfo;
            if (property != null && property.GetGetMethod(true).IsStatic)
                return (IEnumerable<object[]>)property.GetValue(null, null);

            var m = member as MethodInfo;
            if (m != null && m.IsStatic)
                return (IEnumerable<object[]>)m.Invoke(null, null);

            throw new Exception($"Member '{member.Name}' must be static to be used with TestCaseSource");
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
            fixture.Class.Type.InvokeAll<TestFixtureSetUpAttribute>(fixture.Instance);
            next();
            fixture.Class.Type.InvokeAll<TestFixtureTearDownAttribute>(fixture.Instance);
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
