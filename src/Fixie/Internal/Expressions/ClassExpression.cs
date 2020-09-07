namespace Fixie.Internal.Expressions
{
    using System;

    public class ClassExpression
    {
        readonly Discovery discovery;

        internal ClassExpression(Discovery discovery)
            => this.discovery = discovery;

        /// <summary>
        /// Limits discovered test classes to those which satisfy the given condition.
        /// Multiple calls to this method establish multiple conditions, all of which
        /// must be satisfied for a class to be considered a test class.
        /// </summary>
        public ClassExpression Where(Func<Type, bool> condition)
        {
            discovery.AddTestClassCondition(condition);
            return this;
        }
    }
}