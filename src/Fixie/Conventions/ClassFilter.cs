using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie.Conventions
{
    public class ClassFilter
    {
        readonly List<Func<Type, bool>> conditions;
        Random shuffler;

        public ClassFilter()
        {
            conditions = new List<Func<Type, bool>>();

            ConcreteClasses();
        }

        public ClassFilter Where(Func<Type, bool> condition)
        {
            conditions.Add(condition);
            return this;
        }

        public ClassFilter Has<TAttribute>() where TAttribute : Attribute
        {
            return Where(type => type.Has<TAttribute>());
        }

        public ClassFilter HasOrInherits<TAttribute>() where TAttribute : Attribute
        {
            return Where(type => type.HasOrInherits<TAttribute>());
        }

        public ClassFilter NameEndsWith(string suffix)
        {
            return Where(type => type.Name.EndsWith(suffix));
        }

        public ClassFilter Shuffle(Random random)
        {
            shuffler = random;
            return this;
        }

        public ClassFilter Shuffle()
        {
            return Shuffle(new Random());
        }

        public IEnumerable<Type> Filter(IEnumerable<Type> candidates)
        {
            var classes = candidates.Where(IsMatch).ToArray();

            if (shuffler != null)
                classes.Shuffle(shuffler);

            return classes;
        }

        bool IsMatch(Type candidate)
        {
            return conditions.All(condition => condition(candidate));
        }

        void ConcreteClasses()
        {
            Where(type => type.IsClass && !type.IsAbstract);
        }
    }
}