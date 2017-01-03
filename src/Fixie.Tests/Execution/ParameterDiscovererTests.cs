namespace Fixie.Tests.Execution
{
    using System.Collections.Generic;
    using System.Reflection;
    using Fixie.Execution;
    using Should;

    public class ParameterDiscovererTests
    {
        readonly MethodInfo method;

        public ParameterDiscovererTests()
        {
            method = typeof(SampleTestClass).GetInstanceMethod("ParameterizedMethod");
        }

        public void ShouldProvideZeroSetsOfInputParametersByDefault()
        {
            var customConvention = new Convention();

            DiscoveredParameters(customConvention).ShouldBeEmpty();
        }

        public void ShouldProvideSetsOfInputsGeneratedByNamedParameterSources()
        {
            var customConvention = new Convention();

            customConvention
                .Parameters
                .Add<FirstParameterSource>()
                .Add<SecondParameterSource>();

            DiscoveredParameters(customConvention)
                .ShouldEqual(new[]
                {
                    new object[] { "ParameterizedMethod", 0, false },
                    new object[] { "ParameterizedMethod", 1, true },
                    new object[] { "ParameterizedMethod", 2, false },
                    new object[] { "ParameterizedMethod", 3, true }
                });
        }

        public void ShouldProvideSetsOfInputsGeneratedByInstantiatedParameterSources()
        {
            var customConvention = new Convention();

            customConvention
                .Parameters
                .Add(new FirstParameterSource())
                .Add(new SecondParameterSource());

            DiscoveredParameters(customConvention)
                .ShouldEqual(new[]
                {
                    new object[] { "ParameterizedMethod", 0, false },
                    new object[] { "ParameterizedMethod", 1, true },
                    new object[] { "ParameterizedMethod", 2, false },
                    new object[] { "ParameterizedMethod", 3, true }
                });
        }

        public void ShouldProvideSetsOfInputsGeneratedByFuncParameterSources()
        {
            var customConvention = new Convention();

            customConvention
                .Parameters
                .Add(m => new[]
                {
                    new object[] { m.Name, 0, false },
                })
                .Add(m => new[]
                {
                    new object[] { m.Name, 1, false },
                    new object[] { m.Name, 2, true }
                });

            DiscoveredParameters(customConvention)
                .ShouldEqual(new[]
                {
                    new object[] { "ParameterizedMethod", 0, false },
                    new object[] { "ParameterizedMethod", 1, false },
                    new object[] { "ParameterizedMethod", 2, true }
                });
        }

        IEnumerable<object[]> DiscoveredParameters(Convention convention)
        {
            return new ParameterDiscoverer(convention).GetParameters(method);
        }

        class SampleTestClass
        {
            public void ParameterizedMethod(string s, int x, bool b) { }
        }

        class FirstParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                yield return new object[] { method.Name, 0, false };
                yield return new object[] { method.Name, 1, true };
            }
        }

        class SecondParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                yield return new object[] { method.Name, 2, false };
                yield return new object[] { method.Name, 3, true };
            }
        }
    }
}