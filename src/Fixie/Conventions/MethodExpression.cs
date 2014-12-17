using System;
using System.Reflection;

namespace Fixie.Conventions
{
    public class MethodExpression
    {
        readonly Configuration config;

        internal MethodExpression(Configuration config)
        {
            this.config = config;
        }

        public MethodExpression Where(Func<MethodInfo, bool> condition)
        {
            config.AddTestMethodCondition(condition);
            return this;
        }

        public MethodExpression Has<TAttribute>() where TAttribute : Attribute
        {
            return Where(method => method.Has<TAttribute>());
        }

        public MethodExpression HasOrInherits<TAttribute>() where TAttribute : Attribute
        {
            return Where(method => method.HasOrInherits<TAttribute>());
        }
    }
}