namespace Fixie.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ParameterDiscoverer
    {
        readonly ParameterSource[] parameterSources;

        public ParameterDiscoverer(Convention convention)
        {
            parameterSources = convention.Config.ParameterSources
                .Select(sourceType => sourceType())
                .ToArray();
        }

        public IEnumerable<object[]> GetParameters(MethodInfo method)
        {
            return parameterSources.SelectMany(source => source.GetParameters(method));
        }
    }
}