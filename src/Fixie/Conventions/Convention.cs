using System;
using System.Collections.Generic;
using System.Reflection;
using Fixie.Behaviors;

namespace Fixie.Conventions
{
    public abstract class Convention
    {
        protected Convention()
        {
            Fixtures = new ClassFilter();
            Cases = new MethodFilter().Where(m => !m.IsDispose());
            CaseExecutionBehavior = new Invoke();
        }

        public ClassFilter Fixtures { get; private set; }
        public MethodFilter Cases { get; private set; }
        public MethodBehavior CaseExecutionBehavior { get; set; }

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
            {
                var classExecutionBehavior = new CreateInstancePerCase();

                classExecutionBehavior.Execute(fixtureClass, this, listener);
            }
        }
    }
}