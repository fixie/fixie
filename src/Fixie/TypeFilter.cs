using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie
{
    public class TypeFilter
    {
        readonly List<Func<Type, bool>> conditions;

        public TypeFilter()
        {
            conditions = new List<Func<Type, bool>>();
        }

        public TypeFilter Where(Func<Type, bool> condition)
        {
            conditions.Add(condition);
            return this;
        }

        public TypeFilter ConcreteClasses()
        {
            return Where(type => type.IsClass && !type.IsAbstract);
        }

        public TypeFilter HasDefaultConstructor()
        {
            return Where(type => type.GetConstructor(Type.EmptyTypes) != null);
        }

        public TypeFilter NameEndsWith(string suffix)
        {
            return Where(type => type.Name.EndsWith(suffix));
        }

        public IEnumerable<Type> Filter(IEnumerable<Type> candidates)
        {
            return candidates.Where(IsMatch);
        }

        private bool IsMatch(Type candidate)
        {
            return conditions.All(condition => condition(candidate));
        }
    }
}