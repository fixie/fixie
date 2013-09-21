using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Tests.TestClasses
{
    public class ParameterizedMethodTests
    {
        public void ShouldAllowConventionToGeneratePotentiallyManySetsOfInputParametersPerMethod()
        {
            var listener = new StubListener();

            var convention = new SelfTestConvention();
            convention.Parameters(YieldCases);

            convention.Execute(listener, typeof(ParameterizedTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestClasses.ParameterizedMethodTests+ParameterizedTestClass.IntArg(0) passed.",
                "Fixie.Tests.TestClasses.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes(1, 1, 2) passed.",
                "Fixie.Tests.TestClasses.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes(1, 2, 3) passed.",
                "Fixie.Tests.TestClasses.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes(5, 5, 11) failed: Expected sum of 11 but was 10.",
                "Fixie.Tests.TestClasses.ParameterizedMethodTests+ParameterizedTestClass.ZeroArgs passed.");
        }

        IEnumerable<object[]> YieldCases(MethodInfo method)
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