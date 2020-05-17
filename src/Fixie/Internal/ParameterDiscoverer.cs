namespace Fixie.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class ParameterDiscoverer
    {
        readonly IReadOnlyList<ParameterSource> parameterSources;

        public ParameterDiscoverer(Discovery discovery)
            => parameterSources = discovery.Config.ParameterSources;

        public IEnumerable<object?[]> GetParameters(MethodInfo method)
            => parameterSources.SelectMany(source => source.GetParameters(method));
    }
}