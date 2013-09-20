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

            Methods
                .Where(method => method.IsVoid() || method.IsAsync());

            ClassExecution
                .SortCases((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));
        }
    }
}