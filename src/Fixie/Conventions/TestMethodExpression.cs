using System;
using System.Reflection;

namespace Fixie.Conventions
{
    public class TestMethodExpression
    {
        readonly ConfigModel config;

        public TestMethodExpression(ConfigModel config)
        {
            this.config = config;
        }

        public TestMethodExpression Where(Func<MethodInfo, bool> condition)
        {
            config.AddTestMethodCondition(condition);
            return this;
        }

        public TestMethodExpression Has<TAttribute>() where TAttribute : Attribute
        {
            return Where(method => method.Has<TAttribute>());
        }

        public TestMethodExpression HasOrInherits<TAttribute>() where TAttribute : Attribute
        {
            return Where(method => method.HasOrInherits<TAttribute>());
        }
    }
}