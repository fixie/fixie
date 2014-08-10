using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Discovery
{
    public class ParameterDiscoverer
    {
        readonly ParameterSource[] parameterSources;

        public ParameterDiscoverer(Configuration config)
        {
            parameterSources = config.ParameterSourceTypes
                .Select(customBehavior => (ParameterSource)Activator.CreateInstance((Type)customBehavior))
                .ToArray();
        }

        public IEnumerable<object[]> GetParameters(MethodInfo method)
        {
            //TODO: Shortcut when the method has no parameters anyway?
            return parameterSources.SelectMany(source => source.GetParameters(method));
        }
    }
}