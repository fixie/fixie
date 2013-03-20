using System;
using System.Reflection;

namespace Fixie
{
    public class MethodCase : Case
    {
        private readonly Type fixtureClass;
        private readonly MethodInfo method;

        public MethodCase(Type fixtureClass, MethodInfo method)
        {
            this.fixtureClass = fixtureClass;
            this.method = method;
        }

        public string Name
        {
            get { return fixtureClass.FullName + "." + method.Name; }
        }

        public Result Execute(Listener listener)
        {
            var instance = Activator.CreateInstance(fixtureClass);

            try
            {
                method.Invoke(instance, null);
                return Result.Pass;
            }
            catch (TargetInvocationException ex)
            {
                listener.CaseFailed(this, ex.InnerException);
                return Result.Fail;
            }
        }
    }
}