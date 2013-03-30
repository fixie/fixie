using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fixie
{
    public abstract class Convention
    {
        protected Convention()
        {
            Fixtures = new TypeFilter().ConcreteClasses();
            Cases = new MethodFilter();
        }

        protected TypeFilter Fixtures { get; private set; }
        protected MethodFilter Cases { get; private set; }

        public IEnumerable<Type> FixtureClasses(Type[] candidates)
        {
            return Fixtures.Filter(candidates);
        }

        public IEnumerable<MethodInfo> CaseMethods(Type fixtureClass)
        {
            return Cases.Filter(fixtureClass);
        }
    }
}