namespace Fixie.Samples.Skipped
{
    using System;
    using System.Reflection;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .SortMethods((methodA, methodB) => String.Compare(methodA.Name, methodB.Name, StringComparison.Ordinal));

            CaseExecution
                .Skip(SkipDueToClassLevelSkipAttribute, @case => "Whole class skipped")
                .Skip(SkipDueToMethodLevelSkipAttribute);

            ClassExecution
                .Lifecycle<CreateInstancePerClass>();
        }

        static bool SkipDueToClassLevelSkipAttribute(MethodInfo testMethod)
            => testMethod.DeclaringType.Has<SkipAttribute>();

        static bool SkipDueToMethodLevelSkipAttribute(MethodInfo testMethod)
            => testMethod.Has<SkipAttribute>();
    }
}