namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Assertions;

    public class ParameterizationTests : InstrumentedExecutionTests
    {
        class ParameterizedExecution : IExecution
        {
            readonly Func<Test, IEnumerable<object?[]>> parameterSource;

            public ParameterizedExecution(Func<Test, IEnumerable<object?[]>> parameterSource)
                => this.parameterSource = parameterSource;

            public async Task Run(TestSuite testSuite)
            {
                foreach (var test in testSuite.Tests)
                    foreach (var parameters in parameterSource(test))
                        await test.Run(parameters);
            }
        }

        class IsolatedParameterizedExecution : IExecution
        {
            readonly Func<Test, IEnumerable<object?[]>> parameterSource;

            public IsolatedParameterizedExecution(Func<Test, IEnumerable<object?[]>> parameterSource)
                => this.parameterSource = parameterSource;

            public async Task Run(TestSuite testSuite)
            {
                foreach (var test in testSuite.Tests)
                {
                    try
                    {
                        foreach (var parameters in parameterSource(test))
                            await test.Run(parameters);
                    }
                    catch (Exception exception)
                    {
                        await test.Fail(exception);
                    }
                }
            }
        }

        public async Task ShouldAllowRunningTestsMultipleTimesWithVaryingInputParameters()
        {
            var execution = new ParameterizedExecution(InputAttributeOrDefaultParameterSource);
            var output = await RunAsync<ParameterizedTestClass>(execution);

            output.ShouldHaveResults(
                "ParameterizedTestClass.IntArg(0) passed",
                "ParameterizedTestClass.Sum(1, 1, 2) passed",
                "ParameterizedTestClass.Sum(1, 2, 3) passed",
                "ParameterizedTestClass.Sum(5, 5, 11) failed: Expected sum of 11 but was 10.",
                "ParameterizedTestClass.ZeroArgs passed");

            output.ShouldHaveLifecycle("IntArg", "Sum", "Sum", "Sum", "ZeroArgs");
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
            var output = await RunAsync<ParameterizedTestClass>(execution);

            output.ShouldHaveResults(
                "ParameterizedTestClass.IntArg failed: Parameter count mismatch.",
                "ParameterizedTestClass.IntArg(0) passed",
                "ParameterizedTestClass.IntArg(0, 1) failed: Parameter count mismatch.",
                "ParameterizedTestClass.IntArg(0, 1, 2) failed: Parameter count mismatch.",
                "ParameterizedTestClass.IntArg(0, 1, 2, 3) failed: Parameter count mismatch.",

                "ParameterizedTestClass.Sum failed: Parameter count mismatch.",
                "ParameterizedTestClass.Sum(0) failed: Parameter count mismatch.",
                "ParameterizedTestClass.Sum(0, 1) failed: Parameter count mismatch.",
                "ParameterizedTestClass.Sum(0, 1, 2) failed: Expected sum of 2 but was 1.",
                "ParameterizedTestClass.Sum(0, 1, 2, 3) failed: Parameter count mismatch.",

                "ParameterizedTestClass.ZeroArgs passed",
                "ParameterizedTestClass.ZeroArgs(0) failed: Parameter count mismatch.",
                "ParameterizedTestClass.ZeroArgs(0, 1) failed: Parameter count mismatch.",
                "ParameterizedTestClass.ZeroArgs(0, 1, 2) failed: Parameter count mismatch.",
                "ParameterizedTestClass.ZeroArgs(0, 1, 2, 3) failed: Parameter count mismatch.");

            output.ShouldHaveLifecycle("IntArg", "Sum", "ZeroArgs");
        }

        public async Task ShouldSupportEndingTheRunEarlyWhenParameterGenerationThrows()
        {
            var execution = new ParameterizedExecution(BuggyParameterSource);
            var output = await RunAsync<ParameterizedTestClass>(execution);

            output.ShouldHaveResults(
                "ParameterizedTestClass.IntArg(0) passed",
                "ParameterizedTestClass.IntArg(1) failed: Expected 0, but was 1",
                "ParameterizedTestClass.IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg",

                "ParameterizedTestClass.Sum failed: Exception thrown while attempting to yield input parameters for method: IntArg",
                "ParameterizedTestClass.Sum skipped: This test did not run.",

                "ParameterizedTestClass.ZeroArgs failed: Exception thrown while attempting to yield input parameters for method: IntArg",
                "ParameterizedTestClass.ZeroArgs skipped: This test did not run.");

            output.ShouldHaveLifecycle("IntArg", "IntArg");
        }

        public async Task ShouldSupportIsolatingFailuresToTheAffectedTestMethodWhenParameterGenerationThrows()
        {
            var execution = new IsolatedParameterizedExecution(BuggyParameterSource);
            var output = await RunAsync<ParameterizedTestClass>(execution);

            output.ShouldHaveResults(
                "ParameterizedTestClass.IntArg(0) passed",
                "ParameterizedTestClass.IntArg(1) failed: Expected 0, but was 1",
                "ParameterizedTestClass.IntArg failed: Exception thrown while attempting to yield input parameters for method: IntArg",

                "ParameterizedTestClass.Sum(0) failed: Parameter count mismatch.",
                "ParameterizedTestClass.Sum(1) failed: Parameter count mismatch.",
                "ParameterizedTestClass.Sum failed: Exception thrown while attempting to yield input parameters for method: Sum",

                "ParameterizedTestClass.ZeroArgs(0) failed: Parameter count mismatch.",
                "ParameterizedTestClass.ZeroArgs(1) failed: Parameter count mismatch.",
                "ParameterizedTestClass.ZeroArgs failed: Exception thrown while attempting to yield input parameters for method: ZeroArgs");

            output.ShouldHaveLifecycle("IntArg", "IntArg");
        }

        public async Task ShouldFailWithGenericTestNameWhenGenericTypeParametersCannotBeResolved()
        {
            var execution = new IsolatedParameterizedExecution(BuggyParameterSource);
            var output = await RunAsync<ConstrainedGenericTestClass>(execution);

            output.ShouldHaveResults(
                "ConstrainedGenericTestClass.ConstrainedGeneric<System.Int32>(0) passed",
                "ConstrainedGenericTestClass.ConstrainedGeneric<System.Int32>(1) passed",
                "ConstrainedGenericTestClass.ConstrainedGeneric<T> failed: Exception thrown while attempting to yield input parameters for method: ConstrainedGeneric",
                "ConstrainedGenericTestClass.UnconstrainedGeneric<System.Int32>(0) passed",
                "ConstrainedGenericTestClass.UnconstrainedGeneric<System.Int32>(1) passed",
                "ConstrainedGenericTestClass.UnconstrainedGeneric<T> failed: Exception thrown while attempting to yield input parameters for method: UnconstrainedGeneric");

            output.ShouldHaveLifecycle(
                "ConstrainedGeneric", "ConstrainedGeneric",
                "UnconstrainedGeneric", "UnconstrainedGeneric");
        }

        public async Task ShouldResolveGenericTypeParameters()
        {
            var execution = new IsolatedParameterizedExecution(InputAttributeOrDefaultParameterSource);
            var output = await RunAsync<GenericTestClass>(execution);

            output.ShouldHaveResults(
                "GenericTestClass.ConstrainedGeneric<System.Int32>(1) passed",
                "GenericTestClass.ConstrainedGeneric<T>(\"Oops\") failed: The type parameters for generic method ConstrainedGeneric could not be resolved.",

                "GenericTestClass.ConstrainedGenericMethodWithNoInputsProvided<T> failed: Cannot create an instance of T because Type.ContainsGenericParameters is true.",

                "GenericTestClass.GenericMethodWithIncorrectParameterCountProvided<System.Int32>(123, 123) failed: Parameter count mismatch.",

                "GenericTestClass.GenericMethodWithNoInputsProvided<T>(null) failed: The type parameters for generic method GenericMethodWithNoInputsProvided could not be resolved.",

                "GenericTestClass.MultipleGenericArgumentsMultipleParameters<T1, T2>(123, null, 456, System.Int32, System.Object) failed: The type parameters for generic method MultipleGenericArgumentsMultipleParameters could not be resolved.",
                "GenericTestClass.MultipleGenericArgumentsMultipleParameters<System.Int32, System.String>(123, \"stringArg1\", 456, System.Int32, System.String) passed",
                "GenericTestClass.MultipleGenericArgumentsMultipleParameters<T1, T2>(\"stringArg\", null, null, System.String, System.Object) failed: The type parameters for generic method MultipleGenericArgumentsMultipleParameters could not be resolved.",
                "GenericTestClass.MultipleGenericArgumentsMultipleParameters<T1, T2>(\"stringArg1\", null, \"stringArg2\", System.String, System.Object) failed: The type parameters for generic method MultipleGenericArgumentsMultipleParameters could not be resolved.",
                "GenericTestClass.MultipleGenericArgumentsMultipleParameters<System.String, System.String>(null, \"stringArg1\", \"stringArg2\", System.String, System.String) passed",

                "GenericTestClass.SingleGenericArgument<System.Int32>(123, System.Int32) passed",
                "GenericTestClass.SingleGenericArgument<T>(null, System.Object) failed: The type parameters for generic method SingleGenericArgument could not be resolved.",
                "GenericTestClass.SingleGenericArgument<System.String>(\"stringArg\", System.String) passed",

                "GenericTestClass.SingleGenericArgumentMultipleParameters<System.Int32>(123, 456, System.Int32) passed",
                "GenericTestClass.SingleGenericArgumentMultipleParameters<System.String>(\"stringArg\", 123, System.Object) failed: Object of type 'System.Int32' cannot be converted to type 'System.String'.",
                "GenericTestClass.SingleGenericArgumentMultipleParameters<System.Int32>(123, \"stringArg\", System.Object) failed: Object of type 'System.String' cannot be converted to type 'System.Int32'.",
                "GenericTestClass.SingleGenericArgumentMultipleParameters<System.Int32>(123, null, System.Int32) passed", //MethodInfo.Invoke converts nulls to default(T) for value types.
                "GenericTestClass.SingleGenericArgumentMultipleParameters<T>(null, null, System.Object) failed: The type parameters for generic method SingleGenericArgumentMultipleParameters could not be resolved.",
                "GenericTestClass.SingleGenericArgumentMultipleParameters<System.String>(\"stringArg\", null, System.String) passed",
                "GenericTestClass.SingleGenericArgumentMultipleParameters<System.String>(\"stringArg1\", \"stringArg2\", System.String) passed",
                "GenericTestClass.SingleGenericArgumentMultipleParameters<System.String>(null, \"stringArg\", System.String) passed");

            output.ShouldHaveLifecycle(
                "ConstrainedGeneric",

                "MultipleGenericArgumentsMultipleParameters",
                "MultipleGenericArgumentsMultipleParameters",

                "SingleGenericArgument",
                "SingleGenericArgument",

                "SingleGenericArgumentMultipleParameters",
                "SingleGenericArgumentMultipleParameters",
                "SingleGenericArgumentMultipleParameters",
                "SingleGenericArgumentMultipleParameters",
                "SingleGenericArgumentMultipleParameters");
        }

        public async Task ShouldResolveGenericTypeParametersAppearingWithinComplexParameterTypes()
        {
            var execution = new ParameterizedExecution(ComplexGenericParameterSource);
            var output = await RunAsync<ComplexGenericTestClass>(execution);

            output.ShouldHaveResults(
                "ComplexGenericTestClass.CompoundGenericParameter<System.Int32, System.String>([1, A], \"System.Int32\", \"System.String\") passed",
                "ComplexGenericTestClass.CompoundGenericParameter<System.String, System.Int32>([B, 2], \"System.String\", \"System.Int32\") passed",

                "ComplexGenericTestClass.GenericFuncParameter<System.Int32>(5, System.Func`2[System.Int32,System.Int32], 10) passed",
                "ComplexGenericTestClass.GenericFuncParameter<System.String>(5, System.Func`2[System.Int32,System.String], \"5\") passed",

                "ComplexGenericTestClass.GenericFuncParameter<System.String>(5, System.Func`2[System.Int32,System.String], '5') failed: " +
                "Object of type 'System.Char' cannot be converted to type 'System.String'.");

            output.ShouldHaveLifecycle(
                "CompoundGenericParameter", "CompoundGenericParameter",
                "GenericFuncParameter", "GenericFuncParameter");
        }

        static IEnumerable<object?[]> InputAttributeOrDefaultParameterSource(Test test)
        {
            var attributes = test.GetAll<InputAttribute>();

            if (attributes.Any())
            {
                foreach (var input in attributes)
                    yield return input.Parameters;
            }
            else
            {
                yield return test.Parameters.Select(p => Default(p.ParameterType)).ToArray();
            }
        }

        static IEnumerable<object[]> BuggyParameterSource(Test test)
        {
            yield return new object[] { 0 };
            yield return new object[] { 1 };
            throw new Exception("Exception thrown while attempting to yield input parameters for method: " + test.Method.Name);
        }

        static IEnumerable<object[]> ComplexGenericParameterSource(Test test)
        {
            if (test.Name.EndsWith(".CompoundGenericParameter"))
            {
                yield return new object[] {new KeyValuePair<int, string>(1, "A"), "System.Int32", "System.String"};
                yield return new object[] {new KeyValuePair<string, int>("B", 2), "System.String", "System.Int32"};
            }
            else if (test.Name.EndsWith(".GenericFuncParameter"))
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
                WhereAmI();

                if (i != 0)
                    throw new Exception("Expected 0, but was " + i);
            }

            [Input(1, 1, 2)]
            [Input(1, 2, 3)]
            [Input(5, 5, 11)]
            public void Sum(int a, int b, int expectedSum)
            {
                WhereAmI();

                if (a + b != expectedSum)
                    throw new Exception($"Expected sum of {expectedSum} but was {a + b}.");
            }

            public void ZeroArgs()
            {
                WhereAmI();
            }
        }

        class GenericTestClass
        {
            [Input(1)]
            [Input("Oops")]
            public void ConstrainedGeneric<T>(T input) where T : struct
            {
                WhereAmI();
                typeof(T).IsValueType.ShouldBe(true);
            }

            public void ConstrainedGenericMethodWithNoInputsProvided<T>(T input) where T : struct
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            [Input(123, 123)]
            public void GenericMethodWithIncorrectParameterCountProvided<T>(T genericArgument)
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            public void GenericMethodWithNoInputsProvided<T>(T genericArgument)
            {
                WhereAmI();
                throw new ShouldBeUnreachableException();
            }

            [Input(123, null, 456, typeof(int), typeof(object))]
            [Input(123, "stringArg1", 456, typeof(int), typeof(string))]
            [Input("stringArg", null, null, typeof(string), typeof(object))]
            [Input("stringArg1", null, "stringArg2", typeof(string), typeof(object))]
            [Input(null, "stringArg1", "stringArg2", typeof(string), typeof(string))]
            public void MultipleGenericArgumentsMultipleParameters<T1, T2>(T1 genericArgument1A, T2 genericArgument2, T1 genericArgument1B, Type expectedT1, Type expectedT2)
            {
                WhereAmI();
                typeof(T1).ShouldBe(expectedT1);
                typeof(T2).ShouldBe(expectedT2);
            }

            [Input(123, typeof(int))]
            [Input(null, typeof(object))]
            [Input("stringArg", typeof(string))]
            public void SingleGenericArgument<T>(T genericArgument, Type expectedT)
            {
                WhereAmI();
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
                WhereAmI();
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
                WhereAmI();
                typeof(TKey).FullName.ShouldBe(expectedKeyType);
                typeof(TValue).FullName.ShouldBe(expectedValueType);
            }

            public void GenericFuncParameter<TResult>(int input, Func<int, TResult> transform, TResult expectedResult)
            {
                WhereAmI();

                var result = transform(input);

                result.ShouldBe(expectedResult);
            }
        }

        class ConstrainedGenericTestClass
        {
            public void ConstrainedGeneric<T>(T input) where T : struct
            {
                WhereAmI();
                typeof(T).IsValueType.ShouldBe(true);
            }

            public void UnconstrainedGeneric<T>(T input)
            {
                WhereAmI();
            }
        }
    }
}