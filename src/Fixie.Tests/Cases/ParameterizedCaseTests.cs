namespace Fixie.Tests.Cases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Assertions;
    using Fixie.Internal;
    using static Utility;

    public class ParameterizedCaseTests
    {
        readonly Discovery discovery = new SelfTestDiscovery();
        readonly Execution execution = new DefaultExecution();

        public void ShouldAllowDiscoveryToGeneratePotentiallyManySetsOfInputParametersPerMethod()
        {
            discovery.Parameters.Add<InputAttributeOrDefaultParameterSource>();

            Run<ParameterizedTestClass>(discovery, execution)
                .ShouldBe(
                    For<ParameterizedTestClass>(
                        ".IntArg(0) passed",
                        ".MultipleCasesFromAttributes(1, 1, 2) passed",
                        ".MultipleCasesFromAttributes(1, 2, 3) passed",
                        ".MultipleCasesFromAttributes(5, 5, 11) failed: Expected sum of 11 but was 10.",
                        ".ZeroArgs passed"));
        }

        public void ShouldFailWithClearExplanationWhenInputParameterGenerationHasNotBeenCustomizedYetTestMethodAcceptsParameters()
        {
            Run<ParameterizedTestClass>(discovery, execution)
                .ShouldBe(
                    For<ParameterizedTestClass>(
                        ".IntArg failed: This test has declared parameters, but no parameter values have been provided to it.",
                        ".MultipleCasesFromAttributes failed: This test has declared parameters, but no parameter values have been provided to it.",
                        ".ZeroArgs passed"));
        }

        public void ShouldFailWithClearExplanationWhenInputParameterGenerationHasBeenCustomizedYetYieldsZeroSetsOfInputs()
        {
            discovery.Parameters.Add<EmptyParameterSource>();

            Run<ParameterizedTestClass>(discovery, execution)
                .ShouldBe(
                    For<ParameterizedTestClass>(
                        ".IntArg failed: This test has declared parameters, but no parameter values have been provided to it.",
                        ".MultipleCasesFromAttributes failed: This test has declared parameters, but no parameter values have been provided to it.",
                        ".ZeroArgs passed"));
        }

        public void ShouldFailWithClearExplanationWhenParameterCountsAreMismatched()
        {
            discovery.Parameters.Add(new FixedParameterSource(new[]
            {
                new object[] { },
                new object[] { 0 },
                new object[] { 0, 1 },
                new object[] { 0, 1, 2 },
                new object[] { 0, 1, 2, 3 }
            }));

            Run<ParameterizedTestClass>(discovery, execution)
                .ShouldBe(
                    For<ParameterizedTestClass>(
                        ".IntArg failed: Parameter count mismatch.",
                        ".IntArg(0) passed",
                        ".IntArg(0, 1) failed: Parameter count mismatch.",
                        ".IntArg(0, 1, 2) failed: Parameter count mismatch.",
                        ".IntArg(0, 1, 2, 3) failed: Parameter count mismatch.",

                        ".MultipleCasesFromAttributes failed: Parameter count mismatch.",
                        ".MultipleCasesFromAttributes(0) failed: Parameter count mismatch.",
                        ".MultipleCasesFromAttributes(0, 1) failed: Parameter count mismatch.",
                        ".MultipleCasesFromAttributes(0, 1, 2) failed: Expected sum of 2 but was 1.",
                        ".MultipleCasesFromAttributes(0, 1, 2, 3) failed: Parameter count mismatch.",

                        ".ZeroArgs passed"));
        }

        public void ShouldFailWithClearExplanationWhenParameterGenerationThrows()
        {
            discovery.Parameters.Add<LazyBuggyParameterSource>();

            Run<ParameterizedTestClass>(discovery, execution)
                .ShouldBe(
                    For<ParameterizedTestClass>(
                        ".IntArg(0) passed",
                        ".IntArg(1) failed: Expected 0, but was 1",
                        ".IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg",

                        ".MultipleCasesFromAttributes(0) failed: Parameter count mismatch.",
                        ".MultipleCasesFromAttributes(1) failed: Parameter count mismatch.",
                        ".MultipleCasesFromAttributes failed: Exception thrown while attempting to yield input parameters for method: MultipleCasesFromAttributes",

                        ".ZeroArgs passed"));
        }

        public void ShouldIsolateFailureToTheAffectedTestMethodWhenEagerParameterGenerationThrows()
        {
            //The EagerBuggyParameterSource attempts to realize a complete set of
            //parameter arrays before returning, but will first throw an exception
            //when trying to handle the IntArg test method. Since IntArg runs first,
            //this test demonstrates how the failure is isolated to that test method.

            discovery.Parameters.Add<EagerBuggyParameterSource>();

            Run<ParameterizedTestClass>(discovery, execution)
                .ShouldBe(
                    For<ParameterizedTestClass>(
                        ".IntArg failed: Exception thrown while attempting to eagerly build input parameters for method: IntArg",

                        ".MultipleCasesFromAttributes(1, 1, 2) passed",
                        ".MultipleCasesFromAttributes(1, 2, 3) passed",
                        ".MultipleCasesFromAttributes(5, 5, 11) failed: Expected sum of 11 but was 10.",
                        ".ZeroArgs passed"));
        }

        public void ShouldFailWithClearExplanationWhenParameterGenerationExceptionPreventsGenericTypeParametersFromBeingResolvable()
        {
            discovery.Parameters.Add<LazyBuggyParameterSource>();

            Run<ConstrainedGenericTestClass>(discovery, execution)
                .ShouldBe(
                    For<ConstrainedGenericTestClass>(
                        ".ConstrainedGeneric<System.Int32>(0) passed",
                        ".ConstrainedGeneric<System.Int32>(1) passed",
                        ".ConstrainedGeneric<T> failed: Exception thrown while attempting to yield input parameters for method: ConstrainedGeneric",
                        ".UnconstrainedGeneric<System.Int32>(0) passed",
                        ".UnconstrainedGeneric<System.Int32>(1) passed",
                        ".UnconstrainedGeneric<T> failed: Exception thrown while attempting to yield input parameters for method: UnconstrainedGeneric"));
        }

        public void ShouldResolveGenericTypeParameters()
        {
            discovery.Parameters.Add<InputAttributeParameterSource>();

            Run<GenericTestClass>(discovery, execution)
                .ShouldBe(
                    For<GenericTestClass>(
                        ".ConstrainedGeneric<System.Int32>(1) passed",
                        ".ConstrainedGeneric<T>(\"Oops\") failed: Could not resolve type parameters for generic method.",

                        ".ConstrainedGenericMethodWithNoInputsProvided<T> failed: This test has declared parameters, but no parameter values have been provided to it.",

                        ".GenericMethodWithIncorrectParameterCountProvided<System.Int32>(123, 123) failed: Parameter count mismatch.",

                        ".GenericMethodWithNoInputsProvided<T> failed: This test has declared parameters, but no parameter values have been provided to it.",

                        ".MultipleGenericArgumentsMultipleParameters<T1, T2>(123, null, 456, System.Int32, System.Object) failed: Could not resolve type parameters for generic method.",
                        ".MultipleGenericArgumentsMultipleParameters<System.Int32, System.String>(123, \"stringArg1\", 456, System.Int32, System.String) passed",
                        ".MultipleGenericArgumentsMultipleParameters<T1, T2>(\"stringArg\", null, null, System.String, System.Object) failed: Could not resolve type parameters for generic method.",
                        ".MultipleGenericArgumentsMultipleParameters<T1, T2>(\"stringArg1\", null, \"stringArg2\", System.String, System.Object) failed: Could not resolve type parameters for generic method.",
                        ".MultipleGenericArgumentsMultipleParameters<System.String, System.String>(null, \"stringArg1\", \"stringArg2\", System.String, System.String) passed",

                        ".SingleGenericArgument<System.Int32>(123, System.Int32) passed",
                        ".SingleGenericArgument<T>(null, System.Object) failed: Could not resolve type parameters for generic method.",
                        ".SingleGenericArgument<System.String>(\"stringArg\", System.String) passed",

                        ".SingleGenericArgumentMultipleParameters<System.Int32>(123, 456, System.Int32) passed",
                        ".SingleGenericArgumentMultipleParameters<System.String>(\"stringArg\", 123, System.Object) failed: Object of type 'System.Int32' cannot be converted to type 'System.String'.",
                        ".SingleGenericArgumentMultipleParameters<System.Int32>(123, \"stringArg\", System.Object) failed: Object of type 'System.String' cannot be converted to type 'System.Int32'.",
                        ".SingleGenericArgumentMultipleParameters<System.Int32>(123, null, System.Int32) passed", //MethodInfo.Invoke converts nulls to default(T) for value types.
                        ".SingleGenericArgumentMultipleParameters<T>(null, null, System.Object) failed: Could not resolve type parameters for generic method.",
                        ".SingleGenericArgumentMultipleParameters<System.String>(\"stringArg\", null, System.String) passed",
                        ".SingleGenericArgumentMultipleParameters<System.String>(\"stringArg1\", \"stringArg2\", System.String) passed",
                        ".SingleGenericArgumentMultipleParameters<System.String>(null, \"stringArg\", System.String) passed"));
        }

        public void ShouldResolveGenericTypeParametersAppearingWithinComplexParameterTypes()
        {
            discovery.Parameters.Add<ComplexGenericParameterSource>();

            Run<ComplexGenericTestClass>(discovery, execution)
                .ShouldBe(
                    For<ComplexGenericTestClass>(
                        ".CompoundGenericParameter<System.Int32, System.String>([1, A], \"System.Int32\", \"System.String\") passed",
                        ".CompoundGenericParameter<System.String, System.Int32>([B, 2], \"System.String\", \"System.Int32\") passed",

                        ".GenericFuncParameter<System.Int32>(5, System.Func`2[System.Int32,System.Int32], 10) passed",
                        ".GenericFuncParameter<System.String>(5, System.Func`2[System.Int32,System.String], \"5\") passed",

                        ".GenericFuncParameter<System.String>(5, System.Func`2[System.Int32,System.String], '5') failed: " +
                        "Object of type 'System.Char' cannot be converted to type 'System.String'."));
        }

        class InputAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object?[]> GetParameters(MethodInfo method)
            {
                var inputAttributes = method.GetCustomAttributes<InputAttribute>(true).ToArray();

                if (inputAttributes.Any())
                    foreach (var input in inputAttributes)
                        yield return input.Parameters;
            }
        }

        class InputAttributeOrDefaultParameterSource : ParameterSource
        {
            public virtual IEnumerable<object?[]> GetParameters(MethodInfo method)
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

        class LazyBuggyParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                yield return new object[] { 0 };
                yield return new object[] { 1 };
                throw new Exception("Exception thrown while attempting to yield input parameters for method: " + method.Name);
            }
        }

        class EagerBuggyParameterSource : InputAttributeOrDefaultParameterSource
        {
            public override IEnumerable<object?[]> GetParameters(MethodInfo method)
            {
                if (method.Name == nameof(ParameterizedTestClass.IntArg))
                    throw new Exception("Exception thrown while attempting to eagerly build input parameters for method: " + method.Name);

                return base.GetParameters(method).ToArray();
            }
        }

        class FixedParameterSource : ParameterSource
        {
            readonly object[][] parameters;

            public FixedParameterSource(object[][] parameters)
                => this.parameters = parameters;

            public IEnumerable<object[]> GetParameters(MethodInfo method)
                => parameters;
        }

        class ComplexGenericParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                if (method.Name == "CompoundGenericParameter")
                {
                    yield return new object[] {new KeyValuePair<int, string>(1, "A"), "System.Int32", "System.String"};
                    yield return new object[] {new KeyValuePair<string, int>("B", 2), "System.String", "System.Int32"};
                }
                else if (method.Name == "GenericFuncParameter")
                {
                    yield return new object[] {5, new Func<int, int>(i => i * 2), 10};
                    yield return new object[] {5, new Func<int, string>(i => i.ToString()), "5"};
                    yield return new object[] {5, new Func<int, string>(i => i.ToString()), '5'};
                }
            }
        }

        static object? Default(Type type)
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
            [Input(123, null, 456, typeof(int), typeof(object))]
            [Input(123, "stringArg1", 456, typeof(int), typeof(string))]
            [Input("stringArg", null, null, typeof(string), typeof(object))]
            [Input("stringArg1", null, "stringArg2", typeof(string), typeof(object))]
            [Input(null, "stringArg1", "stringArg2", typeof(string), typeof(string))]
            public void MultipleGenericArgumentsMultipleParameters<T1, T2>(T1 genericArgument1A, T2 genericArgument2, T1 genericArgument1B, Type expectedT1, Type expectedT2)
            {
                typeof(T1).ShouldBe(expectedT1);
                typeof(T2).ShouldBe(expectedT2);
            }

            [Input(123, 456, typeof(int))]
            [Input("stringArg", 123, typeof(object))]
            [Input(123, "stringArg", typeof(object))]
            [Input(123, null, typeof(int))]
            [Input(null, null, typeof(object))]
            [Input("stringArg", null, typeof(string))]
            [Input("stringArg1", "stringArg2", typeof(string))]
            [Input(null, "stringArg", typeof(string))]
            public void SingleGenericArgumentMultipleParameters<T>(T genericArgument1, T genericArgument2, Type expectedT)
            {
                typeof(T).ShouldBe(expectedT);
            }

            [Input(123, typeof(int))]
            [Input(null, typeof(object))]
            [Input("stringArg", typeof(string))]
            public void SingleGenericArgument<T>(T genericArgument, Type expectedT)
            {
                typeof(T).ShouldBe(expectedT);
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
                typeof(T).IsValueType.ShouldBe(true);
            }

            public void ConstrainedGenericMethodWithNoInputsProvided<T>(T input) where T : struct
            {
                throw new ShouldBeUnreachableException();
            }

            static string Format(object? obj)
            {
                return obj?.ToString() ?? "[null]";
            }
        }

        class ComplexGenericTestClass
        {
            public void CompoundGenericParameter<TKey, TValue>(KeyValuePair<TKey, TValue> pair, string expectedKeyType, string expectedValueType)
            {
                typeof(TKey).FullName.ShouldBe(expectedKeyType);
                typeof(TValue).FullName.ShouldBe(expectedValueType);
            }

            public void GenericFuncParameter<TResult>(int input, Func<int, TResult> transform, TResult expectedResult)
            {
                var result = transform(input);

                result.ShouldBe(expectedResult);
            }
        }

        class ConstrainedGenericTestClass
        {
            public void UnconstrainedGeneric<T>(T input)
            {
            }

            public void ConstrainedGeneric<T>(T input) where T : struct
            {
                typeof(T).IsValueType.ShouldBe(true);
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class InputAttribute : Attribute
        {
            public InputAttribute(params object?[] parameters)
            {
                Parameters = parameters;
            }

            public object?[] Parameters { get; }
        }
    }
}