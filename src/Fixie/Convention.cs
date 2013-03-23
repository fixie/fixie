using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public abstract class Convention
    {
        protected abstract bool ClassIsFixture(Type concreteClass);
        protected abstract MethodInfo[] QueryCaseMethods(Type fixtureClass);

        public IEnumerable<Type> FixtureClasses(Type[] candidateTypes)
        {
            return candidateTypes.Where(type => type.IsClass && !type.IsAbstract && ClassIsFixture(type));
        }

        public IEnumerable<MethodInfo> CaseMethods(Type fixtureClass)
        {
            return QueryCaseMethods(fixtureClass).Where(method => method.DeclaringType != typeof(object));
        }
    }
}