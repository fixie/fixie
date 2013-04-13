using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie
{
    public class ClassFilter
    {
        readonly List<Func<Type, bool>> conditions;

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

        public ClassFilter HasDefaultConstructor()
        {
            return Where(type => type.GetConstructor(Type.EmptyTypes) != null);
        }

        public ClassFilter NameEndsWith(string suffix)
        {
            return Where(type => type.Name.EndsWith(suffix));
        }

        public IEnumerable<Type> Filter(IEnumerable<Type> candidates)
        {
            return candidates.Where(IsMatch);
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