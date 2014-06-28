using System;
using Fixie.Conventions;

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
                    .Skip(@case => @case.Method.Name.StartsWith("Skip"));

            return selfTestConvention;
        }
    }
}