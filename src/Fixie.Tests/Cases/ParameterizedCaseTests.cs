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
            Convention.Parameters.Add<InputAttributeOrDefaultParameterSource>();

            Run<ParameterizedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(0) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(1, 1, 2) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(1, 2, 3) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(5, 5, 11) failed: Expected sum of 11 but was 10.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs passed");
        }

        public void ShouldFailWithClearExplanationWhenInputParameterGenerationHasNotBeenCustomizedYetTestMethodAcceptsParameters()
        {
            Run<ParameterizedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg failed: This test case has declared parameters, but no parameter values have been provided to it.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes failed: This test case has declared parameters, but no parameter values have been provided to it.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs passed");
        }

        public void ShouldFailWithClearExplanationWhenInputParameterGenerationHasBeenCustomizedYetYieldsZeroSetsOfInputs()
        {
            Convention.Parameters.Add<EmptyParameterSource>();

            Run<ParameterizedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg failed: This test case has declared parameters, but no parameter values have been provided to it.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes failed: This test case has declared parameters, but no parameter values have been provided to it.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs passed");
        }

        public void ShouldFailWithClearExplanationWhenParameterCountsAreMismatched()
        {
            FixedParameterSource.Parameters = new[]
            {
                new object[] { },
                new object[] { 0 },
                new object[] { 0, 1 },
                new object[] { 0, 1, 2 },
                new object[] { 0, 1, 2, 3 }
            };

            Convention.Parameters.Add<FixedParameterSource>();

            Run<ParameterizedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(0) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(0, 1) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(0, 1, 2) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(0, 1, 2, 3) failed: Parameter count mismatch.",

                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(0) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(0, 1) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(0, 1, 2) failed: Expected sum of 2 but was 1.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(0, 1, 2, 3) failed: Parameter count mismatch.",

                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs(0) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs(0, 1) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs(0, 1, 2) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs(0, 1, 2, 3) failed: Parameter count mismatch.");
        }

        public void ShouldFailWithClearExplanationWhenParameterGenerationThrows()
        {
            Convention.Parameters.Add<BuggyParameterSource>();

            Run<ParameterizedTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes failed: Exception thrown while attempting to yield input parameters for method: MultipleCasesFromAttributes",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs failed: Exception thrown while attempting to yield input parameters for method: ZeroArgs",

                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(0) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.IntArg(1) failed: Expected 0, but was 1",

                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(0) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.MultipleCasesFromAttributes(1) failed: Parameter count mismatch.",

                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs(0) failed: Parameter count mismatch.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ParameterizedTestClass.ZeroArgs(1) failed: Parameter count mismatch.");
        }

        public void ShouldFailWithClearExplanationWhenParameterGenerationExceptionPreventsGenericTypeParametersFromBeingResolvable()
        {
            Convention.Parameters.Add<BuggyParameterSource>();

            Run<ConstrainedGenericTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+ConstrainedGenericTestClass.ConstrainedGeneric<T> failed: Exception thrown while attempting to yield input parameters for method: ConstrainedGeneric",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ConstrainedGenericTestClass.UnconstrainedGeneric<System.Object> failed: Exception thrown while attempting to yield input parameters for method: UnconstrainedGeneric",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ConstrainedGenericTestClass.ConstrainedGeneric<System.Int32>(0) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ConstrainedGenericTestClass.ConstrainedGeneric<System.Int32>(1) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ConstrainedGenericTestClass.UnconstrainedGeneric<System.Int32>(0) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+ConstrainedGenericTestClass.UnconstrainedGeneric<System.Int32>(1) passed");
        }

        public void ShouldResolveGenericTypeParameters()
        {
            Convention.Parameters.Add<InputAttributeParameterSource>();

            Run<GenericTestClass>();

            Listener.Entries.ShouldEqual(
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.ConstrainedGenericMethodWithNoInputsProvided<T> failed: This test case has declared parameters, but no parameter values have been provided to it.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.GenericMethodWithNoInputsProvided<System.Object> failed: This test case has declared parameters, but no parameter values have been provided to it.",

                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.ConstrainedGeneric<System.Int32>(1) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.ConstrainedGeneric<T>(\"Oops\") failed: Could not resolve type parameters for generic test case.",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.GenericMethodWithIncorrectParameterCountProvided<System.Object>(123, 123) failed: Parameter count mismatch.",

                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.MultipleGenericArgumentsMultipleParameters<System.Int32, System.Object>(123, null, 456, System.Int32, System.Object) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.MultipleGenericArgumentsMultipleParameters<System.Int32, System.String>(123, \"stringArg1\", 456, System.Int32, System.String) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.MultipleGenericArgumentsMultipleParameters<System.String, System.Object>(\"stringArg\", null, null, System.String, System.Object) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.MultipleGenericArgumentsMultipleParameters<System.String, System.Object>(\"stringArg1\", null, \"stringArg2\", System.String, System.Object) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.MultipleGenericArgumentsMultipleParameters<System.String, System.String>(null, \"stringArg1\", \"stringArg2\", System.String, System.String) passed",

                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgument<System.Int32>(123, System.Int32) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgument<System.Object>(null, System.Object) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgument<System.String>(\"stringArg\", System.String) passed",

                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.Int32>(123, 456, System.Int32) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.Object>(\"stringArg\", 123, System.Object) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.Object>(123, \"stringArg\", System.Object) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.Object>(123, null, System.Object) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.Object>(null, null, System.Object) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.String>(\"stringArg\", null, System.String) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.String>(\"stringArg1\", \"stringArg2\", System.String) passed",
                "Fixie.Tests.Cases.ParameterizedCaseTests+GenericTestClass.SingleGenericArgumentMultipleParameters<System.String>(null, \"stringArg\", System.String) passed");
        }

        class InputAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                var inputAttributes = method.GetCustomAttributes<InputAttribute>(true).ToArray();

                if (inputAttributes.Any())
                    foreach (var input in inputAttributes)
                        yield return input.Parameters;
            }
        }

        class InputAttributeOrDefaultParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
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
        }

        class EmptyParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                yield break;
            }
        }

        class BuggyParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                yield return new object[] { 0 };
                yield return new object[] { 1 };
                throw new Exception("Exception thrown while attempting to yield input parameters for method: " + method.Name);
            }
        }

        class FixedParameterSource : ParameterSource
        {
            public static object[][] Parameters { get; set; }

            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                return Parameters;
            }
        }

        static object Default(Type type)
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
                    throw new Exception($"Expected sum of {expectedSum} but was {a + b}.");
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
                typeof(T1).ShouldEqual(expectedT1, $"Expected {Format(genericArgument1A)}+{Format(genericArgument1B)} to resolve to type {expectedT1} but found type {typeof(T1)}");
                typeof(T2).ShouldEqual(expectedT2, $"Expected {Format(genericArgument2)} to resolve to type {expectedT2} but found type {typeof(T2)}");
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
                typeof(T).ShouldEqual(expectedT, $"Expected {Format(genericArgument1)}+{Format(genericArgument2)} to resolve to type {expectedT} but found type {typeof(T)}");
            }

            [Input(123, typeof(int))]
            [Input("stringArg", typeof(string))]
            [Input(null, typeof(object))]
            public void SingleGenericArgument<T>(T genericArgument, Type expectedT)
            {
                typeof(T).ShouldEqual(expectedT, $"Expected {Format(genericArgument)} to resolve to type {expectedT} but found type {typeof(T)}");
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

            [Input(1)]
            [Input("Oops")]
            public void ConstrainedGeneric<T>(T input) where T : struct
            {
                typeof(T).IsValueType.ShouldBeTrue();
            }

            public void ConstrainedGenericMethodWithNoInputsProvided<T>(T input) where T : struct
            {
                throw new ShouldBeUnreachableException();
            }

            static string Format(object obj)
            {
                return obj?.ToString() ?? "[null]";
            }
        }

        class ConstrainedGenericTestClass
        {
            public void UnconstrainedGeneric<T>(T input)
            {
            }

            public void ConstrainedGeneric<T>(T input) where T : struct
            {
                typeof(T).IsValueType.ShouldBeTrue();
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class InputAttribute : Attribute
        {
            public InputAttribute(params object[] parameters)
            {
                Parameters = parameters;
            }

            public object[] Parameters { get; }
        }
    }
}