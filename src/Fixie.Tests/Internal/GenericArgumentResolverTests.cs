namespace Fixie.Tests.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using Assertions;
using Fixie.Internal;

public class GenericArgumentResolverTests
{
    static readonly Type[] Empty = { };

    public void ShouldResolveNothingWhenThereAreNoInputParameters()
    {
        Resolve("NoParameters")
            .ShouldBe(Empty);
    }

    public void ShouldResolveNothingWhenThereAreNoGenericParameters()
    {
        Resolve("NoGenericArguments", 0, "")
            .ShouldBe(Empty);
    }

    public void ShouldNotResolveWhenGenericTypeHasNoMatchingParameters()
    {
        Resolve("NoMatchingParameters", 0, "")
            .ShouldSatisfy(x => x.ShouldBeGenericTypeParameter("T"));
    }

    public void ShouldNotResolveWhenGenericTypeHasOneNullMatchingParameter()
    {
        Resolve("OneMatchingParameter", new object?[] {null})
            .ShouldSatisfy(t => t.ShouldBeGenericTypeParameter("T"));
    }
        
    public void ShouldResolveToConcreteTypeOfValueWhenGenericTypeHasOneNonNullMatchingParameter()
    {
        Resolve("OneMatchingParameter", 1.2m)
            .ShouldBe(typeof(decimal));

        Resolve("OneMatchingParameter", "string")
            .ShouldBe(typeof(string));
    }

    public void ShouldResolveToFirstConcreteTypeWhenGenericTypeHasMultipleMatchingParametersOfInconsistentConcreteTypes()
    {
        Resolve("MultipleMatchingParameter", 1.2m, "string", 0)
            .ShouldBe(typeof(decimal));
        
        Resolve("MultipleMatchingParameter", 1.2m, "string a", "string b")
            .ShouldBe(typeof(decimal));
    }

    public void ShouldResolveToConcreteTypeOfValuesWhenGenericTypeHasMultipleMatchingParametersOfTheExactSameConcreteType()
    {
        Resolve("MultipleMatchingParameter", 1.2m, 2.3m, 3.4m)
            .ShouldBe(typeof(decimal));

        Resolve("MultipleMatchingParameter", "string a", "string b", "string c")
            .ShouldBe(typeof(string));
    }

    public void ShouldNotResolveWhenGenericTypeHasMultipleMatchingParametersButAllAreNull()
    {
        Resolve("MultipleMatchingParameter", null, null, null)
            .ShouldSatisfy(x => x.ShouldBeGenericTypeParameter("T"));
    }

    public void ShouldTreatNullsAsTypeCompatibleWithReferenceTypes()
    {
        Resolve("MultipleMatchingParameter", null, "string b", "string c")
            .ShouldBe(typeof(string));

        Resolve("MultipleMatchingParameter", "string a", null, "string c")
            .ShouldBe(typeof(string));

        Resolve("MultipleMatchingParameter", "string a", "string b", null)
            .ShouldBe(typeof(string));
    }

    public void ShouldIgnoreNullAsTypeIncompatibleWithValueTypes()
    {
        Resolve("MultipleMatchingParameter", null, 2.3m, 3.4m)
            .ShouldBe(typeof(decimal));

        Resolve("MultipleMatchingParameter", 1.2m, null, 3.4m)
            .ShouldBe(typeof(decimal));

        Resolve("MultipleMatchingParameter", 1.2m, 2.3m, null)
            .ShouldBe(typeof(decimal));
    }

    public void ShouldResolveGenericArgumentsIfAndOnlyIfTheyCanAllBeResolved()
    {
        Resolve("MultipleUnsatisfiableGenericArguments", null, 1.2m, "string", 0)
            .ShouldSatisfy(
                x => x.ShouldBeGenericTypeParameter("TNoMatch"),
                x => x.ShouldBeGenericTypeParameter("TOneMatch"),
                x => x.ShouldBeGenericTypeParameter("TMultipleMatch"));

        Resolve("MultipleUnsatisfiableGenericArguments", false, 1.2m, "string", 0)
            .ShouldSatisfy(
                x => x.ShouldBeGenericTypeParameter("TNoMatch"),
                x => x.ShouldBeGenericTypeParameter("TOneMatch"),
                x => x.ShouldBeGenericTypeParameter("TMultipleMatch"));

        Resolve("MultipleUnsatisfiableGenericArguments", false, 1.2m, "string a", "string b")
            .ShouldSatisfy(
                x => x.ShouldBeGenericTypeParameter("TNoMatch"),
                x => x.ShouldBeGenericTypeParameter("TOneMatch"),
                x => x.ShouldBeGenericTypeParameter("TMultipleMatch"));

        Resolve("MultipleUnsatisfiableGenericArguments", false, 1.2m, 2.3m, 3.4m)
            .ShouldSatisfy(
                x => x.ShouldBeGenericTypeParameter("TNoMatch"),
                x => x.ShouldBeGenericTypeParameter("TOneMatch"),
                x => x.ShouldBeGenericTypeParameter("TMultipleMatch"));

        Resolve("MultipleUnsatisfiableGenericArguments", false, "string a", "string b", "string c")
            .ShouldSatisfy(
                x => x.ShouldBeGenericTypeParameter("TNoMatch"),
                x => x.ShouldBeGenericTypeParameter("TOneMatch"),
                x => x.ShouldBeGenericTypeParameter("TMultipleMatch"));

        Resolve("MultipleUnsatisfiableGenericArguments", false, null, null, null)
            .ShouldSatisfy(
                x => x.ShouldBeGenericTypeParameter("TNoMatch"),
                x => x.ShouldBeGenericTypeParameter("TOneMatch"),
                x => x.ShouldBeGenericTypeParameter("TMultipleMatch"));
        
        Resolve("MultipleUnsatisfiableGenericArguments", false, "string a", "string b", null)
            .ShouldSatisfy(
                x => x.ShouldBeGenericTypeParameter("TNoMatch"),
                x => x.ShouldBeGenericTypeParameter("TOneMatch"),
                x => x.ShouldBeGenericTypeParameter("TMultipleMatch"));

        Resolve("MultipleUnsatisfiableGenericArguments", false, 1.2m, 2.3m, null)
            .ShouldSatisfy(
                x => x.ShouldBeGenericTypeParameter("TNoMatch"),
                x => x.ShouldBeGenericTypeParameter("TOneMatch"),
                x => x.ShouldBeGenericTypeParameter("TMultipleMatch"));

        Resolve("MultipleSatisfiableGenericArguments", null, 1.2m, "string", 0)
            .ShouldSatisfy(
                x => x.ShouldBeGenericTypeParameter("TOneMatch"),
                x => x.ShouldBeGenericTypeParameter("TMultipleMatch"));

        Resolve("MultipleSatisfiableGenericArguments", false, 1.2m, "string", 0)
            .ShouldBe(typeof(bool), typeof(decimal));

        Resolve("MultipleSatisfiableGenericArguments", false, 1.2m, "string a", "string b")
            .ShouldBe(typeof(bool), typeof(decimal));

        Resolve("MultipleSatisfiableGenericArguments", false, 1.2m, 2.3m, 3.4m)
            .ShouldBe(typeof(bool), typeof(decimal));

        Resolve("MultipleSatisfiableGenericArguments", false, "string a", "string b", "string c")
            .ShouldBe(typeof(bool), typeof(string));

        Resolve("MultipleSatisfiableGenericArguments", false, null, null, null)
            .ShouldSatisfy(
                x => x.ShouldBeGenericTypeParameter("TOneMatch"),
                x => x.ShouldBeGenericTypeParameter("TMultipleMatch"));
        
        Resolve("MultipleSatisfiableGenericArguments", false, "string a", "string b", null)
            .ShouldBe(typeof(bool), typeof(string));

        Resolve("MultipleSatisfiableGenericArguments", false, 1.2m, 2.3m, null)
            .ShouldBe(typeof(bool), typeof(decimal));
    }

    public void ShouldNotResolveWhenInputParameterCountIsLessThanDeclaredParameterCount()
    {
        Resolve("MultipleMatchingParameter", 1, 2)
            .ShouldSatisfy(x => x.ShouldBeGenericTypeParameter("T"));
    }

    public void ShouldAttemptReasonableResolutionByIgnoringExcessParametersWhenInputParameterCountIsGreaterThanDeclaredParameterCount()
    {
        Resolve("MultipleMatchingParameter", 1, 2, 3, 4)
            .ShouldBe(typeof(int));
    }

    public void ShouldResolveGenericArgumentsWhenGenericConstraintsAreSatisfied()
    {
        Resolve("ConstrainedGeneric", 1)
            .ShouldBe(typeof(int));

        Resolve("ConstrainedGeneric", true)
            .ShouldBe(typeof(bool));
    }

    public void ShouldResolveGenericTypeParametersAppearingWithinComplexParameterTypes()
    {
        Resolve("CompoundGenericParameter", new KeyValuePair<int, string>(1, "A"))
            .ShouldBe(typeof(int), typeof(string));

        Resolve("CompoundGenericParameter", new KeyValuePair<string, int>("A", 1))
            .ShouldBe(typeof(string), typeof(int));

        Resolve("GenericFuncParameter", 5, new Func<int, int>(i => i * 2), 10)
            .ShouldBe(typeof(int));

        Resolve("GenericFuncParameter", 5, new Func<int, string>(i => i.ToString()), "5")
            .ShouldBe(typeof(string));

        //We select string as our T, though the char argument would fail to cast at runtime,
        //causing this test to fail.
        Resolve("GenericFuncParameter", 5, new Func<int, string>(i => i.ToString()), '5')
            .ShouldBe(typeof(string));
    }

    public void ShouldResolveGenericTypeParametersAppearingWithinArrays()
    {
        Resolve("GenericArrayResolution", new[] {1}, "A")
            .ShouldBe(typeof(int), typeof(string));

        Resolve("GenericArrayResolution", new[] {"B"}, 2)
            .ShouldBe(typeof(string), typeof(int));

        Resolve("GenericArrayResolution", new[] {"C"}, new[] {3})
            .ShouldBe(typeof(string), typeof(int[]));

        Resolve("GenericArrayResolution", 0, 1)
            .ShouldSatisfy(
                x => x.ShouldBeGenericTypeParameter("T1"),
                x => x.ShouldBeGenericTypeParameter("T2"));
    }

    public void ShouldResolveNullableValueTypeParametersWithConcreteValueTypes()
    {
        Resolve("NullableValueTypeResolution", 1, 2, 3, 4)
            .ShouldBe(typeof(int), typeof(int));

        Resolve("NullableValueTypeResolution", 'a', 2.0d, 3.0d, 4)
            .ShouldBe(typeof(char), typeof(double));
        
        Resolve("NullableValueTypeResolution", 'a', 2.0d, 3, 4)
            .ShouldBe(typeof(char), typeof(double));
        
        Resolve("NullableValueTypeResolution", 'a', 2, 3.03, 4)
            .ShouldBe(typeof(char), typeof(int));

        Resolve("NullableValueTypeResolution", 'a', null, 3.03, 4)
            .ShouldBe(typeof(char), typeof(double));

        Resolve("NullableValueTypeResolution", 'a', 2, null, 4)
            .ShouldBe(typeof(char), typeof(int));

        Resolve("NullableValueTypeResolution", null, 2, 3, 4)
            .ShouldSatisfy(
                x => x.ShouldBeGenericTypeParameter("T1"),
                x => x.ShouldBeGenericTypeParameter("T2"));
    }

    public void ShouldLeaveGenericTypeParameterWhenGenericTypeParametersCannotBeResolved()
    {
        var unresolved = Resolve("ConstrainedGeneric", "Incompatible").Single();
        unresolved.Name.ShouldBe("T");
        unresolved.IsGenericParameter.ShouldBe(true);
    }

    static IEnumerable<Type> Resolve(string methodName, params object?[] parameters)
    {
        return typeof(Generic)
            .GetInstanceMethod(methodName)
            .TryResolveTypeArguments(parameters)
            .GetGenericArguments();
    }

    class Generic
    {
        public void NoParameters() { }
        public void NoGenericArguments(int i, string s) { }
        public void NoMatchingParameters<T>(int i, string s) { }
        public void OneMatchingParameter<T>(T match) { }
        public void MultipleMatchingParameter<T>(T firstMatch, T secondMatch, T thirdMatch) { }
        public void MultipleUnsatisfiableGenericArguments<TNoMatch, TOneMatch, TMultipleMatch>(
            TOneMatch oneMatch,
            TMultipleMatch firstMultiMatch, TMultipleMatch secondMultiMatch, TMultipleMatch thirdMultiMatch) { }
        public void MultipleSatisfiableGenericArguments<TOneMatch, TMultipleMatch>(
            TOneMatch oneMatch,
            TMultipleMatch firstMultiMatch, TMultipleMatch secondMultiMatch, TMultipleMatch thirdMultiMatch) { }
        void ConstrainedGeneric<T>(T t) where T : struct { }
        public void CompoundGenericParameter<TKey, TValue>(KeyValuePair<TKey, TValue> pair) { }
        public void GenericFuncParameter<TResult>(int input, Func<int, TResult> transform, TResult expectedResult) { }
        public void GenericArrayResolution<T1, T2>(T1[] array, T2 arbitrary) { }
        public void NullableValueTypeResolution<T1, T2>(T1? a, T2? b, T2? c, int? i)
            where T1: struct
            where T2: struct { }
    }
}