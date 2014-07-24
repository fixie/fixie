using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Should;

namespace Fixie.Tests.Cases
{
    public class ParameterizedCaseTests : CaseTests
    {
        public void ShouldAllowConventionToGeneratePotentiallyManySetsOfInputParametersPerMethod()
        {
            Convention.Parameters(ParametersFromAttributesWithTypeDefaultFallback);

            Run<ParameterizedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(0) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(1, 1, 2) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(1, 2, 3) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(5, 5, 11) failed: Expected sum of 11 but was 10.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs passed.");
        }

        public void ShouldFailWithClearExplanationWhenInputParameterGenerationHasNotBeenCustomizedYetTestMethodAcceptsParameters()
        {
            Run<ParameterizedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs passed.");
        }

        public void ShouldFailWithClearExplanationWhenInputParameterGenerationHasBeenCustomizedYetYieldsZeroSetsOfInputs()
        {
            Convention.Parameters(ZeroSetsOfInputParameters);

            Run<ParameterizedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs passed.");
        }

        public void ShouldFailWithClearExplanationWhenParameterCountsAreMismatched()
        {
            Convention.Parameters(Inputs(
                new object[] { },
                new object[] { 0 },
                new object[] { 0, 1 },
                new object[] { 0, 1, 2 },
                new object[] { 0, 1, 2, 3 }));

            Run<ParameterizedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(0) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(0, 1) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(0, 1, 2) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(0, 1, 2, 3) failed: Parameter count mismatch.",

                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(0) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(0, 1) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(0, 1, 2) failed: Expected sum of 2 but was 1.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(0, 1, 2, 3) failed: Parameter count mismatch.",

                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs(0) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs(0, 1) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs(0, 1, 2) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs(0, 1, 2, 3) failed: Parameter count mismatch.");
        }

        public void ShouldFailWithClearExplanationWhenParameterGenerationThrows()
        {
            Convention.Parameters(ParametersFromBuggySource);

            Run<ParameterizedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(0) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(1) failed: Expected 0, but was 1",

                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(0) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(1) failed: Parameter count mismatch.",

                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs(0) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs(1) failed: Parameter count mismatch.",

                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes failed: Exception thrown while attempting to yield input parameters for method: MultipleCasesFromAttributes",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs failed: Exception thrown while attempting to yield input parameters for method: ZeroArgs");
        }

        public void ShouldResolveGenericTypeParameters()
        {
            Convention.Parameters(ParametersFromAttributes);

            Run<GenericTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.GenericMethodWithIncorrectParameterCountProvided<System.Object>(123, 123) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.GenericMethodWithNoInputsProvided<System.Object> failed: Parameter count mismatch.",

                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.MultipleGenericArgumentsMultipleParameters<System.Int32, System.Object>(123, null, 456, System.Int32, System.Object) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.MultipleGenericArgumentsMultipleParameters<System.Int32, System.String>(123, \"stringArg1\", 456, System.Int32, System.String) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.MultipleGenericArgumentsMultipleParameters<System.String, System.Object>(\"stringArg\", null, null, System.String, System.Object) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.MultipleGenericArgumentsMultipleParameters<System.String, System.Object>(\"stringArg1\", null, \"stringArg2\", System.String, System.Object) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.MultipleGenericArgumentsMultipleParameters<System.String, System.String>(null, \"stringArg1\", \"stringArg2\", System.String, System.String) passed.",

                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgument<System.Int32>(123, System.Int32) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgument<System.Object>(null, System.Object) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgument<System.String>(\"stringArg\", System.String) passed.",

                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.Int32>(123, 456, System.Int32) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.Object>(\"stringArg\", 123, System.Object) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.Object>(123, \"stringArg\", System.Object) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.Object>(123, null, System.Object) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.Object>(null, null, System.Object) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.String>(\"stringArg\", null, System.String) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.String>(\"stringArg1\", \"stringArg2\", System.String) passed.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.String>(null, \"stringArg\", System.String) passed.");
        }

        IEnumerable<object[]> ParametersFromAttributes(MethodInfo method)
        {
            var inputAttributes = method.GetCustomAttributes<InputAttribute>(true).ToArray();

            if (inputAttributes.Any())
                foreach (var input in inputAttributes)
                    yield return input.Parameters;
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

        IEnumerable<object[]> ParametersFromBuggySource(MethodInfo method)
        {
            yield return new object[] { 0 };
            yield return new object[] { 1 };
            throw new Exception("Exception thrown while attempting to yield input parameters for method: " + method.Name);
        }

        Func<MethodInfo, IEnumerable<object[]>> Inputs(params object[][] inputs)
        {
            return method => inputs;
        }

        object Default(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        class ParameterizedTestClass
        {
            public void ZeroArgs() { }

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

        class GenericTestClass
        {
            [Input(null, "stringArg1", "stringArg2", typeof(string), typeof(string))]
            [Input("stringArg", null, null, typeof(string), typeof(object))]
            [Input(123, null, 456, typeof(int), typeof(object))]
            [Input("stringArg1", null, "stringArg2", typeof(string), typeof(object))]
            [Input(123, "stringArg1", 456, typeof(int), typeof(string))]
            public void MultipleGenericArgumentsMultipleParameters<T1, T2>(T1 genericArgument1A, T2 genericArgument2, T1 genericArgument1B, Type expectedT1, Type expectedT2)
            {
                typeof(T1).ShouldEqual(expectedT1, string.Format("Expected {0}+{1} to resolve to type {2} but found type {3}", Format(genericArgument1A), Format(genericArgument1B), expectedT1, typeof(T1)));
                typeof(T2).ShouldEqual(expectedT2, string.Format("Expected {0} to resolve to type {1} but found type {2}", Format(genericArgument2), expectedT2, typeof(T2)));
            }

            [Input(123, 456, typeof(int))]
            [Input(123, null, typeof(object))]
            [Input(null, null, typeof(object))]
            [Input("stringArg1", "stringArg2", typeof(string))]
            [Input(123, "stringArg", typeof(object))]
            [Input("stringArg", 123, typeof(object))]
            [Input(null, "stringArg", typeof(string))]
            [Input("stringArg", null, typeof(string))]
            public void SingleGenericArgumentMultipleParameters<T>(T genericArgument1, T genericArgument2, Type expectedT)
            {
                typeof(T).ShouldEqual(expectedT, string.Format("Expected {0}+{1} to resolve to type {2} but found type {3}", Format(genericArgument1), Format(genericArgument2), expectedT, typeof(T)));
            }

            [Input(123, typeof(int))]
            [Input("stringArg", typeof(string))]
            [Input(null, typeof(object))]
            public void SingleGenericArgument<T>(T genericArgument, Type expectedT)
            {
                typeof(T).ShouldEqual(expectedT, string.Format("Expected {0} to resolve to type {1} but found type {2}", Format(genericArgument), expectedT, typeof(T)));
            }

            [Input(123, 123)]
            public void GenericMethodWithIncorrectParameterCountProvided<T>(T genericArgument)
            {
                throw new ShouldBeUnreachableException();
            }

            public void GenericMethodWithNoInputsProvided<T>(T genericArgument)
            {
                throw new ShouldBeUnreachableException();
            }

            static string Format(object obj)
            {
                return obj == null ? "[null]" : obj.ToString();
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