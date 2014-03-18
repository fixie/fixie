using System;
using Fixie.Conventions;

namespace Fixie.Behaviors
{
    public class CreateInstancePerTestClass : TypeBehavior
    {
        readonly Func<Type, object> construct;

        public CreateInstancePerTestClass(Func<Type, object> construct)
        {
            this.construct = construct;
        }

        public void Execute(TestClass testClass, Convention convention, CaseExecution[] caseExecutions)
        {
            try
            {
                var instance = construct(testClass.Type);

                var testClassInstance = new TestClassInstance(testClass.Type, instance, convention.CaseExecution.Behavior, caseExecutions);
                convention.InstanceExecution.Behavior.Execute(testClassInstance);

                Dispose(instance);
            }
            catch (Exception exception)
            {
                foreach (var caseExecution in caseExecutions)
                    caseExecution.Fail(exception);
            }
        }

        static void Dispose(object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}