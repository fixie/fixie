namespace Fixie.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Assertions;

    public class GenericArgumentResolverTests
    {
        static readonly Type[] Empty = { };

        public void ShouldResolveNothingWhenThereAreNoInputParameters()
        {
            Resolve("NoParameters", new object[] { })
                .ShouldEqual(Empty);
        }

        public void ShouldResolveNothingWhenThereAreNoGenericParameters()
        {
            Resolve("NoGenericArguments", new object[] {0, ""})
                .ShouldEqual(Empty);
        }

        public void ShouldResolveToObjectWhenGenericTypeHasNoMatchingParameters()
        {
            Resolve("NoMatchingParameters", new object[] { 0, "" })
                .ShouldEqual(typeof(object));
        }

        public void ShouldResolveToObjectWhenGenericTypeHasOneNullMatchingParameter()
        {
            Resolve("OneMatchingParameter", new object[] { null })
                .ShouldEqual(typeof(object));
        }

        public void ShouldResolveToConcreteTypeOfValueWhenGenericTypeHasOneNonNullMatchingParameter()
        {
            Resolve("OneMatchingParameter", new object[] { 1.2m })
                .ShouldEqual(typeof(decimal));

            Resolve("OneMatchingParameter", new object[] { "string" })
                .ShouldEqual(typeof(string));
        }

        public void ShouldResolveToObjectWhenGenericTypeHasMultipleMatchingParametersOfInconsistentConcreteTypes()
        {
            Resolve("MultipleMatchingParameter", new object[] { 1.2m, "string", 0 })
                .ShouldEqual(typeof(object));
            
            Resolve("MultipleMatchingParameter", new object[] { 1.2m, "string a", "string b" })
                .ShouldEqual(typeof(object));
        }

        public void ShouldResolveToConcreteTypeOfValuesWhenGenericTypeHasMultipleMatchingParametersOfTheExactSameConcreteType()
        {
            Resolve("MultipleMatchingParameter", new object[] { 1.2m, 2.3m, 3.4m })
                .ShouldEqual(typeof(decimal));

            Resolve("MultipleMatchingParameter", new object[] { "string a", "string b", "string c" })
                .ShouldEqual(typeof(string));
        }

        public void ShouldResolveToObjectWhenGenericTypeHasMultipleMatchingParametersButAllAreNull()
        {
            Resolve("MultipleMatchingParameter", new object[] { null, null, null })
                .ShouldEqual(typeof(object));
        }

        public void ShouldTreatNullsAsTypeCompatibleWithReferenceTypes()
        {
            Resolve("MultipleMatchingParameter", new object[] { null, "string b", "string c" })
                .ShouldEqual(typeof(string));

            Resolve("MultipleMatchingParameter", new object[] { "string a", null, "string c" })
                .ShouldEqual(typeof(string));

            Resolve("MultipleMatchingParameter", new object[] { "string a", "string b", null })
                .ShouldEqual(typeof(string));
        }

        public void ShouldTreatNullAsTypeIncompatibleWithValueTypes()
        {
            Resolve("MultipleMatchingParameter", new object[] { null, 2.3m, 3.4m })
                .ShouldEqual(typeof(object));

            Resolve("MultipleMatchingParameter", new object[] { 1.2m, null, 3.4m })
                .ShouldEqual(typeof(object));

            Resolve("MultipleMatchingParameter", new object[] { 1.2m, 2.3m, null })
                .ShouldEqual(typeof(object));
        }

        public void ShouldResolveAllGenericArguments()
        {
            Resolve("MultipleGenericArguments", new object[] { null, 1.2m, "string", 0 })
                .ShouldEqual(typeof(object), typeof(object), typeof(object));

            Resolve("MultipleGenericArguments", new object[] {false, 1.2m, "string", 0 })
                .ShouldEqual(typeof(object), typeof(bool), typeof(object));

            Resolve("MultipleGenericArguments", new object[] { false, 1.2m, "string a", "string b" })
                .ShouldEqual(typeof(object), typeof(bool), typeof(object));

            Resolve("MultipleGenericArguments", new object[] {false, 1.2m, 2.3m, 3.4m })
                .ShouldEqual(typeof(object), typeof(bool), typeof(decimal));

            Resolve("MultipleGenericArguments", new object[] {false, "string a", "string b", "string c" })
                .ShouldEqual(typeof(object), typeof(bool), typeof(string));

            Resolve("MultipleGenericArguments", new object[] { false, null, null, null })
                .ShouldEqual(typeof(object), typeof(bool), typeof(object));
            
            Resolve("MultipleGenericArguments", new object[] { false, "string a", "string b", null })
                .ShouldEqual(typeof(object), typeof(bool), typeof(string));

            Resolve("MultipleGenericArguments", new object[] { false, 1.2m, 2.3m, null })
                .ShouldEqual(typeof(object), typeof(bool), typeof(object));
        }

        public void ShouldResolveToObjectWhenInputParameterCountDoesNotMatchDeclaredParameterCount()
        {
            Resolve("MultipleGenericArguments", new object[] { 1 }).ShouldEqual(typeof(object), typeof(object), typeof(object));
            Resolve("MultipleGenericArguments", new object[] { 1, true, false, true, false }).ShouldEqual(typeof(object), typeof(object), typeof(object));
        }

        static IEnumerable<Type> Resolve(string methodName, object[] parameters)
        {
            var testClass = typeof(Generic);
            var caseMethod = testClass.GetInstanceMethod(methodName);

            return new Case(testClass, caseMethod, parameters).Method.GetGenericArguments();
        }

        class Generic
        {
            public void NoParameters() { }
            public void NoGenericArguments(int i, string s) { }
            public void NoMatchingParameters<T>(int i, string s) { }
            public void OneMatchingParameter<T>(T match) { }
            public void MultipleMatchingParameter<T>(T firstMatch, T secondMatch, T thirdMatch) { }
            public void MultipleGenericArguments<TNoMatch, TOneMatch, TMultipleMatch>(
                TOneMatch oneMatch,
                TMultipleMatch firstMultiMatch, TMultipleMatch secondMultiMatch, TMultipleMatch thirdMultiMatch) { }
        }
    }
}