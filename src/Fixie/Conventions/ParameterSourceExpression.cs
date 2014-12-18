using System;
using Fixie.Internal;

namespace Fixie.Conventions
{
    public class ParameterSourceExpression
    {
        readonly Configuration config;

        internal ParameterSourceExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Identifies the given type as an generator of test method parameters.
        /// All such registered parameter sources will be asked to contribute parameters to test methods.
        /// </summary>
        public ParameterSourceExpression Add<TParameterSource>() where TParameterSource : ParameterSource
        {
            config.AddParameterSource(() => (ParameterSource)Activator.CreateInstance(typeof(TParameterSource)));
            return this;
        }

        /// <summary>
        /// Identifies the given object as an generator of test method parameters.
        /// All such registered parameter sources will be asked to contribute parameters to test methods.
        /// </summary>
        public ParameterSourceExpression Add(ParameterSource parameterSource)
        {
            config.AddParameterSource(() => parameterSource);
            return this;
        }
    }
}