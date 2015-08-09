using System;

namespace Fixie.Samples.Skipped
{
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
                .Skip(SkipDueToExplicitAttribute, @case => "[Explicit] tests run only when they are individually selected for execution.")
                .Skip(SkipDueToClassLevelSkipAttribute, @case => "whole class skipped")
                .Skip(SkipDueToMethodLevelSkipAttribute);

            ClassExecution
                .CreateInstancePerClass()
                .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));
        }

        bool SkipDueToExplicitAttribute(Case @case)
        {
            var method = @case.Method;

            var isMarkedExplicit = method.Has<ExplicitAttribute>();

            return isMarkedExplicit && TargetMember != method;
        }

        static bool SkipDueToClassLevelSkipAttribute(Case @case)
        {
            return @case.Method.DeclaringType.HasOrInherits<SkipAttribute>();
        }

        static bool SkipDueToMethodLevelSkipAttribute(Case @case)
        {
            return @case.Method.HasOrInherits<SkipAttribute>();
        }
    }
}