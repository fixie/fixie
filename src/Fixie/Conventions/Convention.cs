using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public class Convention
    {
        public Convention()
        {
            Fixtures = new ClassFilter();
            Cases = new MethodFilter().Where(m => !m.IsDispose());
            CaseExecutionBehavior = new Invoke();
            FixtureExecutionBehavior = new CreateInstancePerCase();
        }

        public ClassFilter Fixtures { get; private set; }
        public MethodFilter Cases { get; private set; }
        public MethodBehavior CaseExecutionBehavior { get; set; }
        public TypeBehavior FixtureExecutionBehavior { get; set; }

        public IEnumerable<Type> FixtureClasses(Type[] candidates)
        {
            return Fixtures.Filter(candidates);
        }

        public IEnumerable<MethodInfo> CaseMethods(Type fixtureClass)
        {
            return Cases.Filter(fixtureClass);
        }

        public void Execute(Listener listener, params Type[] candidateTypes)
        {
            foreach (var fixtureClass in FixtureClasses(candidateTypes))
                FixtureExecutionBehavior.Execute(fixtureClass, this, listener);
        }
    }
}