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
    }
}