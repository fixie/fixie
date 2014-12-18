using System;
using System.Collections.Generic;
using System.Reflection;
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
        /// Includes the given type as an generator of test method parameters.
        /// All such registered parameter sources will be asked to contribute parameters to test methods.
        /// </summary>
        public ParameterSourceExpression Add<TParameterSource>() where TParameterSource : ParameterSource
        {
            config.AddParameterSource(() => (ParameterSource)Activator.CreateInstance(typeof(TParameterSource)));
            return this;
        }

        /// <summary>
        /// Includes the given object as an generator of test method parameters.
        /// All such registered parameter sources will be asked to contribute parameters to test methods.
        /// </summary>
        public ParameterSourceExpression Add(ParameterSource parameterSource)
        {
            config.AddParameterSource(() => parameterSource);
            return this;
        }

        /// <summary>
        /// Includes the given delegate as an generator of test method parameters.
        /// All such registered parameter sources will be asked to contribute parameters to test methods.
        /// </summary>
        public ParameterSourceExpression Add(Func<MethodInfo, IEnumerable<object[]>> getParameters)
        {
            config.AddParameterSource(() => new LambdaParameterSource(getParameters));
            return this;
        }

        class LambdaParameterSource : ParameterSource
        {
            readonly Func<MethodInfo, IEnumerable<object[]>> getParameters;

            public LambdaParameterSource(Func<MethodInfo, IEnumerable<object[]>> getParameters)
            {
                this.getParameters = getParameters;
            }

            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                return getParameters(method);
            }
        }
    }
}