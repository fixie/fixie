namespace Fixie.Samples.Skipped
{
    using System;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid());

            CaseExecution
                .Skip(SkipDueToClassLevelSkipAttribute, @case => "Whole class skipped")
                .Skip(SkipDueToMethodLevelSkipAttribute);

            ClassExecution
                .Lifecycle<CreateInstancePerClass>()
                .SortMethods((methodA, methodB) => String.Compare(methodA.Name, methodB.Name, StringComparison.Ordinal));
        }

        static bool SkipDueToClassLevelSkipAttribute(Case @case)
            => @case.Method.DeclaringType.Has<SkipAttribute>();

        static bool SkipDueToMethodLevelSkipAttribute(Case @case)
            => @case.Method.Has<SkipAttribute>();
    }
}