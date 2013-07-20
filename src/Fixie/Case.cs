using System;
using System.Reflection;

namespace Fixie
{
    public class Case
    {
        public Case(Type testClass, MethodInfo caseMethod)
        {
            Class = testClass;
            Method = caseMethod;
            Exceptions = new ExceptionList();
        }

        public string Name
        {
            get { return Class.FullName + "." + Method.Name; }
        }

        public Type Class { get; private set; }
        public MethodInfo Method { get; private set; }
        public ExceptionList Exceptions { get; private set; }
    }
}
