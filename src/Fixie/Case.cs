using System;
using System.Reflection;

namespace Fixie
{
    public class Case
    {
        public Case(Type testClass, MemberInfo caseMethod)
        {
            Class = testClass;
            Member = caseMethod;
            Exceptions = new ExceptionList();
        }

        public string Name
        {
            get { return Class.FullName + "." + Member.Name; }
        }

        public Type Class { get; private set; }
        public MemberInfo Member { get; private set; }
        public ExceptionList Exceptions { get; private set; }
    }
}
