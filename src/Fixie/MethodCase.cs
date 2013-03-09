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

        public void Execute()
        {
            var instance = Activator.CreateInstance(fixtureClass);

            method.Invoke(instance, null);
        }
    }
}