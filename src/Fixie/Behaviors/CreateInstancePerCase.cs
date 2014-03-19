using System;

namespace Fixie.Behaviors
{
    public class CreateInstancePerCase : TypeBehavior
    {
        readonly Func<Type, object> construct;

        public CreateInstancePerCase(Func<Type, object> construct)
        {
            this.construct = construct;
        }

        public void Execute(TestClass testClass)
        {
            foreach (var caseExecution in testClass.CaseExecutions)
            {
                try
                {
                    var instance = construct(testClass.Type);

                    var testClassInstance = new TestClassInstance(testClass.Convention, testClass.Type, instance, new[] { caseExecution });
                    testClass.Convention.InstanceExecution.Behavior.Execute(testClassInstance);

                    Dispose(instance);
                }
                catch (Exception exception)
                {
                    caseExecution.Fail(exception);
                }
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