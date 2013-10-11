using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Tests.TestMethods
{
    public class ParameterizedMethodTests
    {
        public void ShouldAllowConventionToGeneratePotentiallyManySetsOfInputParametersPerMethod()
        {
            var listener = new StubListener();

            var convention = new SelfTestConvention();
            convention.Parameters(ParametersFromAttributesWithTypeDefaultFallback);

            convention.Execute(listener, typeof(ParameterizedTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.IntArg(0) passed.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes(1, 1, 2) passed.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes(1, 2, 3) passed.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes(5, 5, 11) failed: Expected sum of 11 but was 10.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.ZeroArgs passed.");
        }

        public void ShouldFailWithClearExplanationWhenInputParameterGenerationHasNotBeenCustomizedYetTestMethodAcceptsParameters()
        {
            var listener = new StubListener();

            var convention = new SelfTestConvention();

            convention.Execute(listener, typeof(ParameterizedTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.IntArg failed: This parameterized test could not be executed, because no input values were available.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes failed: This parameterized test could not be executed, because no input values were available.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.ZeroArgs passed.");
        }

        public void ShouldFailWithClearExplanationWhenInputParameterGenerationHasBeenCustomizedYetYieldsZeroSetsOfInputs()
        {
            var listener = new StubListener();

            var convention = new SelfTestConvention();
            convention.Parameters(ZeroSetsOfInputParameters);

            convention.Execute(listener, typeof(ParameterizedTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.IntArg failed: This parameterized test could not be executed, because no input values were available.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes failed: This parameterized test could not be executed, because no input values were available.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.ZeroArgs passed.");
        }

        IEnumerable<object[]> ParametersFromAttributesWithTypeDefaultFallback(MethodInfo method)
        {
            var parameters = method.GetParameters();

            var inputAttributes = method.GetCustomAttributes<InputAttribute>(true).ToArray();

            if (inputAttributes.Any())
            {
                foreach (var input in inputAttributes)
                    yield return input.Parameters;
            }
            else
            {
                yield return parameters.Select(p => Default(p.ParameterType)).ToArray();
            }
        }

        IEnumerable<object[]> ZeroSetsOfInputParameters(MethodInfo method)
        {
            yield break;
        }

        object Default(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        class ParameterizedTestClass
        {
            public void ZeroArgs()
            {
            }

            public void IntArg(int i)
            {
                if (i != 0)
                    throw new Exception("Expected 0, but was " + i);
            }

            [Input(1, 1, 2)]
            [Input(1, 2, 3)]
            [Input(5, 5, 11)]
            public void MultipleCasesFromAttributes(int a, int b, int expectedSum)
            {
                if (a + b != expectedSum)
                    throw new Exception(string.Format("Expected sum of {0} but was {1}.", expectedSum, a + b));
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class InputAttribute : Attribute
        {
            public InputAttribute(params object[] parameters)
            {
                Parameters = parameters;
            }

            public object[] Parameters { get; private set; }
        }
    }
}