namespace Fixie.Tests
{
    using System;

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
                .Methods
                    .OrderBy((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));

            return selfTestConvention;
        }
    }
}