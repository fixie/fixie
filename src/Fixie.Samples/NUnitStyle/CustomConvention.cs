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
                .Lifecycle<SetUpTearDown>()
                .SortMethods((methodA, methodB) => String.Compare(methodA.Name, methodB.Name, StringComparison.Ordinal));
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
                var sourceType = attribute.SourceType ?? method.DeclaringType;

                if (sourceType == null)
                    throw new Exception("Could not find source type for method " + method.Name);

                var members = sourceType.GetMember(attribute.SourceName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                if (members.Length != 1)
                    throw new Exception($"Found {members.Length} members named '{attribute.SourceName}' on type {sourceType}");

                var member = members.Single();

                testInvocations.AddRange(InvocationsForTestCaseSource(member));
            }

            return testInvocations;
        }

        static IEnumerable<object[]> InvocationsForTestCaseSource(MemberInfo member)
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

    class SetUpTearDown : Lifecycle
    {
        public void Execute(Type testClass, Action<CaseAction> runCases)
        {
            var instance = Activator.CreateInstance(testClass);

            testClass.InvokeAll<TestFixtureSetUpAttribute>(instance);
            runCases(@case =>
            {
                testClass.InvokeAll<SetUpAttribute>(instance);

                @case.Execute(instance);

                HandleExpectedExceptions(@case);

                testClass.InvokeAll<TearDownAttribute>(instance);
            });
            testClass.InvokeAll<TestFixtureTearDownAttribute>(instance);

            (instance as IDisposable)?.Dispose();
        }

        static void HandleExpectedExceptions(Case @case)
        {
            var attribute = @case.Method.GetCustomAttributes<ExpectedExceptionAttribute>(false).SingleOrDefault();

            if (attribute == null)
                return;

            var exception = @case.Exception;

            try
            {
                if (exception == null)
                    throw new Exception("Expected exception of type " + attribute.ExpectedException + ".");

                if (!attribute.ExpectedException.IsAssignableFrom(exception.GetType()))
                {
                    @case.ClearException();

                    throw new Exception(
                        "Expected exception of type " + attribute.ExpectedException + " but an exception of type " +
                        exception.GetType() + " was thrown.", exception);
                }

                if (attribute.ExpectedMessage != null && exception.Message != attribute.ExpectedMessage)
                {
                    @case.ClearException();

                    throw new Exception(
                        "Expected exception message '" + attribute.ExpectedMessage + "'" + " but was '" + exception.Message + "'.",
                        exception);
                }

                @case.ClearException();
            }
            catch (Exception failureException)
            {
                @case.Fail(failureException);
            }
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
