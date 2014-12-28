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
                .Skip(@case => @case.Method.HasOrInherits<SkipAttribute>() || @case.Method.DeclaringType.HasOrInherits<SkipAttribute>(),
                    @case => @case.Method.DeclaringType.HasOrInherits<SkipAttribute>() ? "whole class skipped" : null);

            ClassExecution
                .CreateInstancePerClass()
                .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));
        }
    }
}