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
                .Where(x => x.IsNestedPrivate)
                .Where(x => x.Name.EndsWith("TestClass"));

            selfTestConvention
                .Methods
                .OrderBy(x => x.Name, StringComparer.Ordinal);

            return selfTestConvention;
        }
    }
}