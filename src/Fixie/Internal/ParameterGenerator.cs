namespace Fixie.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class ParameterGenerator
    {
        readonly IReadOnlyList<ParameterSource> parameterSources;

        public ParameterGenerator(Discovery discovery)
            => parameterSources = discovery.Config.ParameterSources;

        public IEnumerable<object?[]> GetParameters(MethodInfo method)
            => parameterSources.SelectMany(source => source.GetParameters(method));
    }
}