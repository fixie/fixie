using System;

namespace Fixie.Tests
{
    public static class SelfTestConvention
    {
        public static Convention Build()
        {
            var selfTestConvention = new Convention();

            selfTestConvention
                .Classes
                    .Where(testClass => testClass.IsNestedPrivate)
                    .NameEndsWith("TestClass");

            selfTestConvention
                .ClassExecution
                    .SortCases((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));

            selfTestConvention
                .CaseExecution
                    .Skip(@case => @case.Method.Name.StartsWith("Skip"), GetSkipReason);

            return selfTestConvention;
        }

        static string GetSkipReason(Case @case)
        {
            return @case.Method.Name.StartsWith("SkipWithReason") ? "Skipped due to naming convention." : null;
        }
    }
}