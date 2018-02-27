namespace Fixie.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

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
        public ParameterSourceExpression Add<TParameterSource>() where TParameterSource : ParameterSource, new()
        {
            config.AddParameterSource(new TParameterSource());
            return this;
        }

        /// <summary>
        /// Includes the given object as an generator of test method parameters.
        /// All such registered parameter sources will be asked to contribute parameters to test methods.
        /// </summary>
        public ParameterSourceExpression Add(ParameterSource parameterSource)
        {
            config.AddParameterSource(parameterSource);
            return this;
        }

        /// <summary>
        /// Includes the given delegate as an generator of test method parameters.
        /// All such registered parameter sources will be asked to contribute parameters to test methods.
        /// 
        /// <para>
        /// Given a test method, yields zero or more sets
        /// of method parameters to be passed into the test method.
        /// Each object array returned represents a distinct
        /// invocation of the test method.
        /// </para>
        /// </summary>
        public ParameterSourceExpression Add(ParameterSourceFunc getParameters)
        {
            config.AddParameterSource(new LambdaParameterSource(getParameters));
            return this;
        }

        class LambdaParameterSource : ParameterSource
        {
            readonly ParameterSourceFunc getParameters;

            public LambdaParameterSource(ParameterSourceFunc getParameters)
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