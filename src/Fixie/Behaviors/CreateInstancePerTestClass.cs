using System;

namespace Fixie.Behaviors
{
    public class CreateInstancePerTestClass : TypeBehavior
    {
        readonly Func<Type, object> construct;

        public CreateInstancePerTestClass(Func<Type, object> construct)
        {
            this.construct = construct;
        }

        public void Execute(TestClass testClass)
        {
            try
            {
                var instance = construct(testClass.Type);

                var testClassInstance = new TestClassInstance(testClass.Type, instance, testClass.Convention.CaseExecution.Behavior, testClass.CaseExecutions);
                testClass.Convention.InstanceExecution.Behavior.Execute(testClassInstance);

                Dispose(instance);
            }
            catch (Exception exception)
            {
                testClass.FailCases(exception);
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