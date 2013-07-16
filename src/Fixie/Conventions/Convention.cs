using System;
using System.Linq;

namespace Fixie.Conventions
{
    public class Convention
    {
        public Convention()
        {
            Classes = new ClassFilter().Where(type => !type.IsSubclassOf(typeof(Convention)));
            Cases = new MethodFilter().Where(m => !m.IsDispose());
            CaseExecution = new MethodBehaviorBuilder();
            InstanceExecution = new InstanceBehaviorBuilder();
            ClassExecution = new TypeBehaviorBuilder().CreateInstancePerCase();
        }

        public ClassFilter Classes { get; private set; }
        public MethodFilter Cases { get; private set; }
        public MethodBehaviorBuilder CaseExecution { get; private set; }
        public InstanceBehaviorBuilder InstanceExecution { get; private set; }
        public TypeBehaviorBuilder ClassExecution { get; private set; }

        public void Execute(Listener listener, params Type[] candidateTypes)
        {
            foreach (var testClass in Classes.Filter(candidateTypes))
            {
                var cases = Cases.Filter(testClass).Select(x => new Case(testClass, x)).ToArray();

                ClassExecution.Behavior.Execute(testClass, this, cases);

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