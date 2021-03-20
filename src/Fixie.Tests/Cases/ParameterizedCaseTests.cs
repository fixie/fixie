namespace Fixie.Tests.Cases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Assertions;
    using static Utility;

    public class ParameterizedCaseTests
    {
        class ParameterizedExecution : Execution
        {
            readonly Func<MethodInfo, IEnumerable<object?[]>> parameterSource;

            public ParameterizedExecution(Func<MethodInfo, IEnumerable<object?[]>> parameterSource)
                => this.parameterSource = parameterSource;

            public async Task RunAsync(TestAssembly testAssembly)
            {
                foreach (var test in testAssembly.Tests)
                    foreach (var parameters in parameterSource(test.Method))
                        await test.RunAsync(parameters);
            }
        }

        class IsolatedParameterizedExecution : Execution
        {
            readonly Func<MethodInfo, IEnumerable<object?[]>> parameterSource;

            public IsolatedParameterizedExecution(Func<MethodInfo, IEnumerable<object?[]>> parameterSource)
                => this.parameterSource = parameterSource;

            public async Task RunAsync(TestAssembly testAssembly)
            {
                foreach (var test in testAssembly.Tests)
                {
                    try
                    {
                        foreach (var parameters in parameterSource(test.Method))
                            await test.RunAsync(parameters);
                    }
                    catch (Exception exception)
                    {
                        await test.FailAsync(exception);
                    }
                }
            }
        }

        public async Task ShouldAllowRunningTestsMultipleTimesWithVaryingInputParameters()
        {
            var execution = new ParameterizedExecution(InputAttributeOrDefaultParameterSource);
            (await RunAsync<ParameterizedTestClass>(execution))
                .ShouldBe(
                    For<ParameterizedTestClass>(
                        ".IntArg(0) passed",
                        ".MultipleCasesFromAttributes(1, 1, 2) passed",
                        ".MultipleCasesFromAttributes(1, 2, 3) passed",
                        ".MultipleCasesFromAttributes(5, 5, 11) failed: Expected sum of 11 but was 10.",
                        ".ZeroArgs passed"));
        }

        public async Task ShouldFailWithClearExplanationWhenParameterCountsAreMismatched()
        {
            var execution = new ParameterizedExecution(method => new[]
            {
                new object[] { },
                new object[] {0},
                new object[] {0, 1},
                new object[] {0, 1, 2},
                new object[] {0, 1, 2, 3}
            });
            (await RunAsync<ParameterizedTestClass>(execution))
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

                        ".ZeroArgs passed",
                        ".ZeroArgs(0) failed: Parameter count mismatch.",
                        ".ZeroArgs(0, 1) failed: Parameter count mismatch.",
                        ".ZeroArgs(0, 1, 2) failed: Parameter count mismatch.",
                        ".ZeroArgs(0, 1, 2, 3) failed: Parameter count mismatch."));
        }

        public async Task ShouldSupportEndingTheRunEarlyWhenParameterGenerationThrows()
        {
            var failFastExecution = new ParameterizedExecution(BuggyParameterSource);
            (await RunAsync<ParameterizedTestClass>(failFastExecution))
                .ShouldBe(
                    For<ParameterizedTestClass>(
                        ".IntArg(0) passed",
                        ".IntArg(1) failed: Expected 0, but was 1",
                        ".IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg",

                        ".MultipleCasesFromAttributes failed: Exception thrown while attempting to yield input parameters for method: IntArg",
                        ".MultipleCasesFromAttributes skipped: This test did not run.",
                        
                        ".ZeroArgs failed: Exception thrown while attempting to yield input parameters for method: IntArg",
                        ".ZeroArgs skipped: This test did not run."));
        }

        public async Task ShouldSupportIsolatingFailuresToTheAffectedTestMethodWhenParameterGenerationThrows()
        {
            var isolatedExecution = new IsolatedParameterizedExecution(BuggyParameterSource);
            (await RunAsync<ParameterizedTestClass>(isolatedExecution))
                .ShouldBe(
                    For<ParameterizedTestClass>(
                        ".IntArg(0) passed",
                        ".IntArg(1) failed: Expected 0, but was 1",
                        ".IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg",

                        ".MultipleCasesFromAttributes(0) failed: Parameter count mismatch.",
                        ".MultipleCasesFromAttributes(1) failed: Parameter count mismatch.",
                        ".MultipleCasesFromAttributes failed: Exception thrown while attempting to yield input parameters for method: MultipleCasesFromAttributes",

                        ".ZeroArgs(0) failed: Parameter count mismatch.",
                        ".ZeroArgs(1) failed: Parameter count mismatch.",
                        ".ZeroArgs failed: Exception thrown while attempting to yield input parameters for method: ZeroArgs"));
        }

        public async Task ShouldFailWithGenericTestNameWhenGenericTypeParametersCannotBeResolved()
        {
            var execution = new IsolatedParameterizedExecution(BuggyParameterSource);
            (await RunAsync<ConstrainedGenericTestClass>(execution))
                .ShouldBe(
                    For<ConstrainedGenericTestClass>(
                        ".ConstrainedGeneric<System.Int32>(0) passed",
                        ".ConstrainedGeneric<System.Int32>(1) passed",
                        ".ConstrainedGeneric<T> failed: Exception thrown while attempting to yield input parameters for method: ConstrainedGeneric",
                        ".UnconstrainedGeneric<System.Int32>(0) passed",
                        ".UnconstrainedGeneric<System.Int32>(1) passed",
                        ".UnconstrainedGeneric<T> failed: Exception thrown while attempting to yield input parameters for method: UnconstrainedGeneric"));
        }

        public async Task ShouldResolveGenericTypeParameters()
        {
            var execution = new IsolatedParameterizedExecution(InputAttributeOrDefaultParameterSource);
            (await RunAsync<GenericTestClass>(execution))
                .ShouldBe(
                    For<GenericTestClass>(
                        ".ConstrainedGeneric<System.Int32>(1) passed",
                        ".ConstrainedGeneric<T>(\"Oops\") failed: Could not resolve type parameters for generic method.",
                        
                        ".ConstrainedGenericMethodWithNoInputsProvided<T> failed: Cannot create an instance of T because Type.ContainsGenericParameters is true.",

                        ".GenericMethodWithIncorrectParameterCountProvided<System.Int32>(123, 123) failed: Parameter count mismatch.",

                        ".GenericMethodWithNoInputsProvided<T>(null) failed: Could not resolve type parameters for generic method.",

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
                        ".SingleGenericArgumentMultipleParameters<System.String>(null, \"stringArg\", System.String) passed"
                        ));
        }

        public async Task ShouldResolveGenericTypeParametersAppearingWithinComplexParameterTypes()
        {
            var execution = new ParameterizedExecution(ComplexGenericParameterSource);
            (await RunAsync<ComplexGenericTestClass>(execution))
                .ShouldBe(
                    For<ComplexGenericTestClass>(
                        ".CompoundGenericParameter<System.Int32, System.String>([1, A], \"System.Int32\", \"System.String\") passed",
                        ".CompoundGenericParameter<System.String, System.Int32>([B, 2], \"System.String\", \"System.Int32\") passed",

                        ".GenericFuncParameter<System.Int32>(5, System.Func`2[System.Int32,System.Int32], 10) passed",
                        ".GenericFuncParameter<System.String>(5, System.Func`2[System.Int32,System.String], \"5\") passed",

                        ".GenericFuncParameter<System.String>(5, System.Func`2[System.Int32,System.String], '5') failed: " +
                        "Object of type 'System.Char' cannot be converted to type 'System.String'."));
        }

        static IEnumerable<object?[]> InputAttributeOrDefaultParameterSource(MethodInfo method)
        {
            var parameters = method.GetParameters();

            var attributes = method.GetCustomAttributes<InputAttribute>(true).ToArray();

            if (attributes.Any())
            {
                foreach (var input in attributes)
                    yield return input.Parameters;
            }
            else
            {
                yield return parameters.Select(p => Default(p.ParameterType)).ToArray();
            }
        }

        static IEnumerable<object[]> BuggyParameterSource(MethodInfo method)
        {
            yield return new object[] { 0 };
            yield return new object[] { 1 };
            throw new Exception("Exception thrown while attempting to yield input parameters for method: " + method.Name);
        }

        static IEnumerable<object[]> ComplexGenericParameterSource(MethodInfo method)
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

        static object? Default(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        class ParameterizedTestClass
        {
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

            public void ZeroArgs() { }
        }

        class GenericTestClass
        {
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

            [Input(123, 123)]
            public void GenericMethodWithIncorrectParameterCountProvided<T>(T genericArgument)
            {
                throw new ShouldBeUnreachableException();
            }

            public void GenericMethodWithNoInputsProvided<T>(T genericArgument)
            {
                throw new ShouldBeUnreachableException();
            }

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

            [Input(123, typeof(int))]
            [Input(null, typeof(object))]
            [Input("stringArg", typeof(string))]
            public void SingleGenericArgument<T>(T genericArgument, Type expectedT)
            {
                typeof(T).ShouldBe(expectedT);
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
            public void ConstrainedGeneric<T>(T input) where T : struct
            {
                typeof(T).IsValueType.ShouldBe(true);
            }

            public void UnconstrainedGeneric<T>(T input)
            {
            }
        }
    }
}