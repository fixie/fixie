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
                .Skip(SkipDueToClassLevelExplicitAttribute, @case => "Tests within [Explicit] classes run only when the class is individually selected for execution.")
                .Skip(SkipDueToMethodLevelExplicitAttribute, @case => "[Explicit] tests run only when they are individually selected for execution.")
                .Skip(SkipDueToClassLevelSkipAttribute, @case => "whole class skipped")
                .Skip(SkipDueToMethodLevelSkipAttribute);

            ClassExecution
                .CreateInstancePerClass()
                .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));
        }

        bool SkipDueToClassLevelExplicitAttribute(Case @case)
        {
            var method = @case.Method;

            var isMarkedExplicit = method.DeclaringType.Has<ExplicitAttribute>();

            return isMarkedExplicit && TargetMember != method.DeclaringType && TargetMember != method;
        }

        bool SkipDueToMethodLevelExplicitAttribute(Case @case)
        {
            var method = @case.Method;

            var isMarkedExplicit = method.Has<ExplicitAttribute>();

            return isMarkedExplicit && TargetMember != method;
        }

        static bool SkipDueToClassLevelSkipAttribute(Case @case)
        {
            return @case.Method.DeclaringType.Has<SkipAttribute>();
        }

        static bool SkipDueToMethodLevelSkipAttribute(Case @case)
        {
            return @case.Method.Has<SkipAttribute>();
        }
    }
}