using System;

namespace Fixie.Conventions
{
    public class ClassExpression
    {
        readonly Configuration config;

        internal ClassExpression(Configuration config)
        {
            this.config = config;
        }

        public ClassExpression Where(Func<Type, bool> condition)
        {
            config.AddTestClassCondition(condition);
            return this;
        }

        public ClassExpression Has<TAttribute>() where TAttribute : Attribute
        {
            return Where(type => type.Has<TAttribute>());
        }

        public ClassExpression HasOrInherits<TAttribute>() where TAttribute : Attribute
        {
            return Where(type => type.HasOrInherits<TAttribute>());
        }

        public ClassExpression NameEndsWith(string suffix)
        {
            return Where(type => type.Name.EndsWith(suffix));
        }
    }
}