using System;
using System.Reflection;

namespace Fixie
{
    public class Case
    {
        readonly Type fixtureClass;

        public Case(Type fixtureClass, MethodInfo caseMethod)
        {
            this.fixtureClass = fixtureClass;
            Method = caseMethod;
            Exceptions = new ExceptionList();
        }

        public string Name
        {
            get { return fixtureClass.FullName + "." + Method.Name; }
        }

        public MethodInfo Method { get; private set; }
        public ExceptionList Exceptions { get; private set; }
    }
}
