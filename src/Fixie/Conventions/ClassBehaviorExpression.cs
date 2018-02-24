namespace Fixie.Conventions
{
    using System;

    public class ClassBehaviorExpression
    {
        readonly Configuration config;

        internal ClassBehaviorExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Overrides the default test class lifecycle.
        /// </summary>
        public ClassBehaviorExpression Lifecycle<TLifecycle>() where TLifecycle : Lifecycle
        {
            config.Lifecycle = (Lifecycle)Activator.CreateInstance(typeof(TLifecycle));
            return this;
        }

        /// <summary>
        /// Overrides the default test class lifecycle.
        /// </summary>
        public ClassBehaviorExpression Lifecycle(Lifecycle lifecycle)
        {
            config.Lifecycle = lifecycle;
            return this;
        }
    }
}