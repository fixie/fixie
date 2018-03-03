namespace Fixie.Conventions
{
    using System;

    public class ClassExpression
    {
        readonly Configuration config;

        internal ClassExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Limits discovered test classes to those which satisfy the given condition.
        /// Multiple calls to this method establish multiple conditions, all of which
        /// must be satisfied for a class to be considered a test class.
        /// </summary>
        public ClassExpression Where(Func<Type, bool> condition)
        {
            config.AddTestClassCondition(condition);
            return this;
        }
    }
}