using System;
using System.Reflection;

namespace Fixie
{
    public class Case
    {
        private readonly Type fixtureClass;
        private readonly MethodInfo method;

        public Case(Type fixtureClass, MethodInfo method)
        {
            this.fixtureClass = fixtureClass;
            this.method = method;
        }

        public void Execute()
        {
            var instance = Activator.CreateInstance(fixtureClass);

            method.Invoke(instance, null);
        }
    }
}