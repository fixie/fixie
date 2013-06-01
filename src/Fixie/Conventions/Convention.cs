using System;
using System.Collections.Generic;
using System.Linq;
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
            CaseExecution = new MethodBehaviorBuilder();
            InstanceExecution = new InstanceBehaviorBuilder();
            FixtureExecution = new TypeBehaviorBuilder().CreateInstancePerCase();
        }

        public ClassFilter Fixtures { get; private set; }
        public MethodFilter Cases { get; private set; }
        public MethodBehaviorBuilder CaseExecution { get; private set; }
        public InstanceBehaviorBuilder InstanceExecution { get; private set; }
        public TypeBehaviorBuilder FixtureExecution { get; private set; }

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
                var cases = CaseMethods(fixtureClass).Select(x => new Case(fixtureClass, x)).ToArray();

                FixtureExecution.Behavior.Execute(fixtureClass, this, cases);

                foreach (var @case in cases)
                {
                    var exceptions = @case.Exceptions;

                    if (exceptions.Any())
                        listener.CaseFailed(@case.Name, exceptions.ToArray());
                    else
                        listener.CasePassed(@case.Name);
                }
            }
        }
    }
}