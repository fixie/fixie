using System.Linq;
using System.Reflection;
using Should;

namespace Fixie.Tests
{
    public class UncallableParameterizedCaseTests
    {
        bool invoked = false;

        public void ShouldAlwaysFailWithoutInvokingTheUnderlyingMethod()
        {
            var @case = UncallableParameterizedCase("Parameterized");

            @case.Execute(this);

            invoked.ShouldBeFalse();

            var exception = @case.Exceptions.ToArray().Single();
            exception.GetType().Name.ShouldEqual("ArgumentException");
            exception.Message.ShouldEqual("This parameterized test could not be executed, because no input values were available.");
        }

        void Parameterized(int x)
        {
            invoked = true;
        }

        static Case UncallableParameterizedCase(string methodName)
        {
            var testClass = typeof(UncallableParameterizedCaseTests);
            return new UncallableParameterizedCase(testClass, testClass.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
        }
    }
}