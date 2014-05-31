using System;

namespace Fixie.Conventions
{
    public class SelfTestConvention : Convention
    {
        public SelfTestConvention()
        {
            Classes
                .Where(testClass => testClass.IsNestedPrivate)
                .NameEndsWith("TestClass");

            ClassExecution
                .SortCases((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));

            CaseExecution
                .Skip(@case => @case.Method.Name.StartsWith("Skip"));
        }
    }
}