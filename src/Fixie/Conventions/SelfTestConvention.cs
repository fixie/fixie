using System;
using System.Globalization;
using System.Threading;

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
                .SetUp(_ => Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US"))
                .SortCases((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));

            CaseExecution
                .Skip(@case => @case.Method.Name.StartsWith("Skip"));
        }
    }
}