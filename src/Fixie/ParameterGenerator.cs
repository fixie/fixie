namespace Fixie
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ParameterGenerator : ParameterSource
    {
        readonly List<ParameterSource> sources = new List<ParameterSource>();

        /// <summary>
        /// Includes the given type as a generator of test method parameters.
        /// All such registered parameter sources will be asked to contribute parameters to test methods.
        /// </summary>
        public ParameterGenerator Add<TParameterSource>() where TParameterSource : ParameterSource, new()
        {
            sources.Add(new TParameterSource());
            return this;
        }

        /// <summary>
        /// Includes the given object as a generator of test method parameters.
        /// All such registered parameter sources will be asked to contribute parameters to test methods.
        /// </summary>
        public ParameterGenerator Add(ParameterSource parameterSource)
        {
            sources.Add(parameterSource);
            return this;
        }

        IEnumerable<object?[]> ParameterSource.GetParameters(MethodInfo method)
            => sources.SelectMany(source => source.GetParameters(method));
    }
}