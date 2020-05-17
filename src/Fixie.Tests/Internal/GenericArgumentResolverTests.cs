namespace Fixie.Tests.Internal
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
            Resolve("NoParameters")
                .ShouldBe(Empty);
        }

        public void ShouldResolveNothingWhenThereAreNoGenericParameters()
        {
            Resolve("NoGenericArguments", 0, "")
                .ShouldBe(Empty);
        }

        public void ShouldResolveToObjectWhenGenericTypeHasNoMatchingParameters()
        {
            Resolve("NoMatchingParameters", 0, "")
                .ShouldBe(typeof(object));
        }

        public void ShouldResolveToObjectWhenGenericTypeHasOneNullMatchingParameter()
        {
            Resolve("OneMatchingParameter", new object?[] { null })
                .ShouldBe(typeof(object));
        }

        public void ShouldResolveToConcreteTypeOfValueWhenGenericTypeHasOneNonNullMatchingParameter()
        {
            Resolve("OneMatchingParameter", 1.2m)
                .ShouldBe(typeof(decimal));

            Resolve("OneMatchingParameter", "string")
                .ShouldBe(typeof(string));
        }

        public void ShouldResolveToObjectWhenGenericTypeHasMultipleMatchingParametersOfInconsistentConcreteTypes()
        {
            Resolve("MultipleMatchingParameter", 1.2m, "string", 0)
                .ShouldBe(typeof(object));
            
            Resolve("MultipleMatchingParameter", 1.2m, "string a", "string b")
                .ShouldBe(typeof(object));
        }

        public void ShouldResolveToConcreteTypeOfValuesWhenGenericTypeHasMultipleMatchingParametersOfTheExactSameConcreteType()
        {
            Resolve("MultipleMatchingParameter", 1.2m, 2.3m, 3.4m)
                .ShouldBe(typeof(decimal));

            Resolve("MultipleMatchingParameter", "string a", "string b", "string c")
                .ShouldBe(typeof(string));
        }

        public void ShouldResolveToObjectWhenGenericTypeHasMultipleMatchingParametersButAllAreNull()
        {
            Resolve("MultipleMatchingParameter", null, null, null)
                .ShouldBe(typeof(object));
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

        public void ShouldTreatNullAsTypeIncompatibleWithValueTypes()
        {
            Resolve("MultipleMatchingParameter", null, 2.3m, 3.4m)
                .ShouldBe(typeof(object));

            Resolve("MultipleMatchingParameter", 1.2m, null, 3.4m)
                .ShouldBe(typeof(object));

            Resolve("MultipleMatchingParameter", 1.2m, 2.3m, null)
                .ShouldBe(typeof(object));
        }

        public void ShouldResolveAllGenericArguments()
        {
            Resolve("MultipleGenericArguments", null, 1.2m, "string", 0)
                .ShouldBe(typeof(object), typeof(object), typeof(object));

            Resolve("MultipleGenericArguments", false, 1.2m, "string", 0)
                .ShouldBe(typeof(object), typeof(bool), typeof(object));

            Resolve("MultipleGenericArguments", false, 1.2m, "string a", "string b")
                .ShouldBe(typeof(object), typeof(bool), typeof(object));

            Resolve("MultipleGenericArguments", false, 1.2m, 2.3m, 3.4m)
                .ShouldBe(typeof(object), typeof(bool), typeof(decimal));

            Resolve("MultipleGenericArguments", false, "string a", "string b", "string c")
                .ShouldBe(typeof(object), typeof(bool), typeof(string));

            Resolve("MultipleGenericArguments", false, null, null, null)
                .ShouldBe(typeof(object), typeof(bool), typeof(object));
            
            Resolve("MultipleGenericArguments", false, "string a", "string b", null)
                .ShouldBe(typeof(object), typeof(bool), typeof(string));

            Resolve("MultipleGenericArguments", false, 1.2m, 2.3m, null)
                .ShouldBe(typeof(object), typeof(bool), typeof(object));
        }

        public void ShouldResolveToObjectWhenInputParameterCountDoesNotMatchDeclaredParameterCount()
        {
            Resolve("MultipleGenericArguments", 1).ShouldBe(typeof(object), typeof(object), typeof(object));
            Resolve("MultipleGenericArguments", 1, true, false, true, false).ShouldBe(typeof(object), typeof(object), typeof(object));
        }

        public void ShouldResolveGenericArgumentsWhenGenericConstraintsAreSatisfied()
        {
            Resolve("ConstrainedGeneric", 1)
                .ShouldBe(typeof(int));

            Resolve("ConstrainedGeneric", true)
                .ShouldBe(typeof(bool));
        }

        public void ShouldLeaveGenericTypeParameterWhenGenericTypeParametersCannotBeResolved()
        {
            var unresolved = Resolve("ConstrainedGeneric", "Incompatible").Single();
            unresolved.Name.ShouldBe("T");
            unresolved.IsGenericParameter.ShouldBe(true);
        }

        static IEnumerable<Type> Resolve(string methodName, params object?[] parameters)
        {
            var testClass = typeof(Generic);
            var caseMethod = testClass.GetInstanceMethod(methodName);

            return new Case(caseMethod, parameters).Method.GetGenericArguments();
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
            void ConstrainedGeneric<T>(T t) where T : struct { }
        }
    }
}