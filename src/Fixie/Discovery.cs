namespace Fixie
{
    using Conventions;

    /// <summary>
    /// Subclass Discovery to customize test discovery and execution.
    /// </summary>
    public abstract class Discovery
    {
        protected Discovery()
        {
            Config = new Configuration();

            Classes = new ClassExpression(Config);
            Methods = new MethodExpression(Config);
            Parameters = new ParameterSourceExpression(Config);
        }

        /// <summary>
        /// The current state describing the discovery rules. This state can be manipulated through
        /// the other properties on Discovery.
        /// </summary>
        internal Configuration Config { get; }

        /// <summary>
        /// Defines the set of conditions that describe which classes are test classes.
        /// </summary>
        public ClassExpression Classes { get; }

        /// <summary>
        /// Defines the set of conditions that describe which test class methods are test methods,
        /// and what order to run them in.
        /// </summary>
        public MethodExpression Methods { get; }

        /// <summary>
        /// Defines the set of parameter sources, which provide inputs to parameterized test methods.
        /// </summary>
        public ParameterSourceExpression Parameters { get; }
    }
}