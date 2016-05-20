namespace Fixie.Tests
{
    using System;
    using System.Reflection;

    public static class SampleTestClassConvention
    {
        public static Convention Build()
        {
            var convention = new Convention();

            convention
                .Classes
                .Where(testClass => testClass == typeof(SampleTestClass));

            convention
                .ClassExecution
                .SortCases((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));

            convention.CaseExecution.Skip(x => x.Method.Has<SkipAttribute>(), x => x.Method.GetCustomAttribute<SkipAttribute>().Reason);

            return convention;
        }
    }
}