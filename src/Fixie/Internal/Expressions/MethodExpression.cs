namespace Fixie.Internal.Expressions
{
    using System;
    using System.Reflection;

    public class MethodExpression
    {
        readonly Discovery discovery;

        internal MethodExpression(Discovery discovery)
            => this.discovery = discovery;

        /// <summary>
        /// Limits discovered test methods to those which satisfy the given condition.
        /// Multiple calls to this method establish multiple conditions, all of which
        /// must be satisfied for a method to be considered a test method.
        /// </summary>
        public MethodExpression Where(Func<MethodInfo, bool> condition)
        {
            discovery.AddTestMethodCondition(condition);
            return this;
        }
    }
}