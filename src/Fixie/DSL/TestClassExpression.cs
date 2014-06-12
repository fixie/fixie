using System;
using Fixie.Conventions;

namespace Fixie.DSL
{
    public class TestClassExpression
    {
        readonly ConfigModel config;

        public TestClassExpression(ConfigModel config)
        {
            this.config = config;
        }

        public TestClassExpression Where(Func<Type, bool> condition)
        {
            config.AddTestClassCondition(condition);
            return this;
        }

        public TestClassExpression Has<TAttribute>() where TAttribute : Attribute
        {
            return Where(type => type.Has<TAttribute>());
        }

        public TestClassExpression HasOrInherits<TAttribute>() where TAttribute : Attribute
        {
            return Where(type => type.HasOrInherits<TAttribute>());
        }

        public TestClassExpression NameEndsWith(string suffix)
        {
            return Where(type => type.Name.EndsWith(suffix));
        }
    }
}