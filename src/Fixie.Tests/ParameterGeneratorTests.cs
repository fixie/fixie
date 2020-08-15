namespace Fixie.Tests
{
    using System.Collections.Generic;
    using System.Reflection;
    using Assertions;

    public class ParameterGeneratorTests
    {
        class SampleDiscovery : Discovery
        {
        }

        readonly MethodInfo method;

        public ParameterGeneratorTests()
        {
            method = typeof(SampleTestClass).GetInstanceMethod("ParameterizedMethod");
        }

        public void ShouldProvideZeroSetsOfInputParametersByDefault()
        {
            var customDiscovery = new SampleDiscovery();

            DiscoveredParameters(customDiscovery).ShouldBeEmpty();
        }

        public void ShouldProvideSetsOfInputsGeneratedByNamedParameterSources()
        {
            var customDiscovery = new SampleDiscovery();

            customDiscovery
                .Parameters
                .Add<FirstParameterSource>()
                .Add<SecondParameterSource>();

            DiscoveredParameters(customDiscovery)
                .ShouldBe(
                    new object[] { "ParameterizedMethod", 0, false },
                    new object[] { "ParameterizedMethod", 1, true },
                    new object[] { "ParameterizedMethod", 2, false },
                    new object[] { "ParameterizedMethod", 3, true });
        }

        public void ShouldProvideSetsOfInputsGeneratedByInstantiatedParameterSources()
        {
            var customDiscovery = new SampleDiscovery();

            customDiscovery
                .Parameters
                .Add(new FirstParameterSource())
                .Add(new SecondParameterSource());

            DiscoveredParameters(customDiscovery)
                .ShouldBe(
                    new object[] { "ParameterizedMethod", 0, false },
                    new object[] { "ParameterizedMethod", 1, true },
                    new object[] { "ParameterizedMethod", 2, false },
                    new object[] { "ParameterizedMethod", 3, true });
        }

        IEnumerable<object?[]> DiscoveredParameters(Discovery discovery)
        {
            return new ParameterGenerator(discovery).GetParameters(method);
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