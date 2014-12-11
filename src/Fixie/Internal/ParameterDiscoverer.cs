using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Internal
{
    public class ParameterDiscoverer
    {
        readonly ParameterSource[] parameterSources;

        public ParameterDiscoverer(Configuration config)
        {
            parameterSources = config.ParameterSourceTypes
                .Select(sourceType => (ParameterSource)Activator.CreateInstance(sourceType))
                .ToArray();
        }

        public IEnumerable<object[]> GetParameters(MethodInfo method)
        {
            return parameterSources.SelectMany(source => source.GetParameters(method));
        }
    }
}