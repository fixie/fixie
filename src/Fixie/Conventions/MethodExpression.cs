using System;
using System.Reflection;
using Fixie.Internal;

namespace Fixie.Conventions
{
    public class MethodExpression
    {
        readonly Configuration config;

        internal MethodExpression(Configuration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Limits discovered test methods to those which satisfy the given condition.
        /// Multiple calls to this method establish multiple conditions, all of which
        /// must be satisfied for a method to be considered a test method.
        /// </summary>
        public MethodExpression Where(Func<MethodInfo, bool> condition)
        {
            config.AddTestMethodCondition(condition);
            return this;
        }

        /// <summary>
        /// Limits discovered test methods to those methods which have the specified attribute.
        /// </summary>
        public MethodExpression Has<TAttribute>() where TAttribute : Attribute
        {
            return Where(method => method.Has<TAttribute>());
        }

        /// <summary>
        /// Limits discovered test methods to those methods which have or inherit the specified attribute.
        /// </summary>
        public MethodExpression HasOrInherits<TAttribute>() where TAttribute : Attribute
        {
            return Where(method => method.HasOrInherits<TAttribute>());
        }
    }
}