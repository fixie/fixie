namespace Fixie
{
    using System;
    using Conventions;

    /// <summary>
    /// Base class for all Fixie conventions. Subclass Convention to customize test discovery and execution.
    /// </summary>
    public class Convention
    {
        public Convention()
        {
            Config = new Configuration();

            Classes = new ClassExpression(Config);
            Methods = new MethodExpression(Config);
            Parameters = new ParameterSourceExpression(Config);
        }

        /// <summary>
        /// The current state describing the convention. This state can be manipulated through
        /// the other properties on Convention.
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

        /// <summary>
        /// Overrides the default test class lifecycle.
        /// </summary>
        [Obsolete]
        public void Lifecycle<TLifecycle>() where TLifecycle : Lifecycle, new()
            => Config.Lifecycle = new TLifecycle();

        /// <summary>
        /// Overrides the default test class lifecycle.
        /// </summary>
        [Obsolete]
        public void Lifecycle(Lifecycle lifecycle)
            => Config.Lifecycle = lifecycle;
    }
}