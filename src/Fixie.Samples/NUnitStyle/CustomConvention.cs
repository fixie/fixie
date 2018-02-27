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
                .Where(x => x.HasOrInherits<TestFixture>());

            Methods
                .Where(x => x.HasOrInherits<Test>())
                .OrderBy(x => x.Name, StringComparer.Ordinal);

            Parameters
                .Add<TestCaseSourceAttributeParameterSource>();

            Lifecycle<SetUpTearDown>();
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
        public void Execute(TestClass testClass, Action<CaseAction> runCases)
        {
            var instance = testClass.Construct();

            testClass.Execute<TestFixtureSetUp>(instance);
            runCases(@case =>
            {
                testClass.Execute<SetUp>(instance);

                @case.Execute(instance);

                HandleExpectedExceptions(@case);

                testClass.Execute<TearDown>(instance);
            });
            testClass.Execute<TestFixtureTearDown>(instance);

            instance.Dispose();
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
                    throw new Exception(
                        "Expected exception of type " + attribute.ExpectedException + " but an exception of type " +
                        exception.GetType() + " was thrown.", exception);
                }

                if (attribute.ExpectedMessage != null && exception.Message != attribute.ExpectedMessage)
                {
                    throw new Exception(
                        "Expected exception message '" + attribute.ExpectedMessage + "'" + " but was '" + exception.Message + "'.",
                        exception);
                }

                @case.Pass();
            }
            catch (Exception failureException)
            {
                @case.Fail(failureException);
            }
        }
    }
}
