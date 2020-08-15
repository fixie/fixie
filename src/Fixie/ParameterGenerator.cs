namespace Fixie
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ParameterGenerator
    {
        readonly IReadOnlyList<ParameterSource> parameterSources;

        internal ParameterGenerator(Discovery discovery)
            => parameterSources = discovery.Config.ParameterSources;

        public IEnumerable<object?[]> GetParameters(MethodInfo method)
            => parameterSources.SelectMany(source => source.GetParameters(method));
    }
}