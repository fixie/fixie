using System;
using System.Reflection;

namespace Fixie
{
    public class Case
    {
        readonly Type testClass;

        public Case(Type testClass, MethodInfo caseMethod)
        {
            this.testClass = testClass;
            Method = caseMethod;
            Exceptions = new ExceptionList();
        }

        public string Name
        {
            get { return testClass.FullName + "." + Method.Name; }
        }

        public MethodInfo Method { get; private set; }
        public ExceptionList Exceptions { get; private set; }
    }
}
