using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Internal
{
    public class ParameterDiscoverer
    {
        readonly ParameterSource[] parameterSources;

        public ParameterDiscoverer(Configuration config)
        {
            parameterSources = config.ParameterSources
                .Select(sourceType => sourceType())
                .ToArray();
        }

        public IEnumerable<object[]> GetParameters(MethodInfo method)
        {
            return parameterSources.SelectMany(source => source.GetParameters(method));
        }
    }
}