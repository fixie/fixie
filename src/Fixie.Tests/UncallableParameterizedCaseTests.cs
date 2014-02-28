using System.Linq;
using Should;

namespace Fixie.Tests
{
    public class UncallableParameterizedCaseTests
    {
        bool invoked = false;

        public void ShouldAlwaysFailWithoutInvokingTheUnderlyingMethod()
        {
            var @case = UncallableParameterizedCase("Parameterized");
            var caseExecution = new CaseExecution(@case);

            @case.Execute(this, caseExecution);

            invoked.ShouldBeFalse();

            var exception = caseExecution.Exceptions.Single();
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
            return new UncallableParameterizedCase(testClass.GetInstanceMethod(methodName));
        }
    }
}