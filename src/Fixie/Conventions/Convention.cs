using System;
using System.Linq;

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

        public void Execute(Listener listener, params Type[] candidateTypes)
        {
            foreach (var fixtureClass in Fixtures.Filter(candidateTypes))
            {
                var cases = Cases.Filter(fixtureClass).Select(x => new Case(fixtureClass, x)).ToArray();

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