namespace Fixie.Execution
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class ParameterDiscoverer
    {
        readonly IReadOnlyList<ParameterSource> parameterSources;

        public ParameterDiscoverer(Convention convention)
            => parameterSources = convention.Config.ParameterSources;

        public IEnumerable<object[]> GetParameters(MethodInfo method)
            => parameterSources.SelectMany(source => source.GetParameters(method));
    }
}