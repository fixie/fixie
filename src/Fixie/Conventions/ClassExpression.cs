using System;
using Fixie.Internal;

namespace Fixie.Conventions
{
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

        /// <summary>
        /// Limits discovered test classes to those classes which have the specified attribute.
        /// </summary>
        public ClassExpression Has<TAttribute>() where TAttribute : Attribute
        {
            return Where(type => type.Has<TAttribute>());
        }

        /// <summary>
        /// Limits discovered test classes to those classes which have or inherit the specified attribute.
        /// </summary>
        public ClassExpression HasOrInherits<TAttribute>() where TAttribute : Attribute
        {
            return Where(type => type.HasOrInherits<TAttribute>());
        }

        /// <summary>
        /// Limits discovered test classes to those whose names end with the given suffix.
        /// </summary>
        public ClassExpression NameEndsWith(string suffix)
        {
            return Where(type => type.Name.EndsWith(suffix));
        }
    }
}