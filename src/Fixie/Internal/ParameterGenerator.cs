namespace Fixie.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class ParameterGenerator : ParameterSource
    {
        readonly IReadOnlyList<ParameterSource> parameterSources;

        public ParameterGenerator(IReadOnlyList<ParameterSource> parameterSources)
            => this.parameterSources = parameterSources;

        public IEnumerable<object?[]> GetParameters(MethodInfo method)
            => parameterSources.SelectMany(source => source.GetParameters(method));
    }
}