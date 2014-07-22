using System;

namespace Fixie.Conventions
{
    public class TestClassExpression
    {
        readonly Configuration config;

        public TestClassExpression(Configuration config)
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