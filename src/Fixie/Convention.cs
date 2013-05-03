using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fixie
{
    public abstract class Convention
    {
        protected Convention()
        {
            Fixtures = new ClassFilter();
            Cases = new MethodFilter().Where(m => !m.IsDispose());
        }

        public ClassFilter Fixtures { get; private set; }
        public MethodFilter Cases { get; private set; }

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