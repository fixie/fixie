namespace Fixie
{
    using Internal.Expressions;

    /// <summary>
    /// Subclass Discovery to customize test discovery rules.
    /// 
    /// The default discovery rules are applied to a test assembly whenever the test
    /// assembly includes no such subclass.
    ///
    /// By defualt,
    /// 
    /// <para>A class is a test class if its name ends with "Tests".</para>
    ///
    /// <para>All public methods in a test class are test methods.</para>
    /// </summary>
    public class Discovery
    {
        public Discovery()
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