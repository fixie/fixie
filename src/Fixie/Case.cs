using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fixie
{
    public class Case
    {
        readonly List<Exception> exceptions;

        public Case(Type testClass, MethodInfo caseMethod)
        {
            Class = testClass;
            Method = caseMethod;
            exceptions = new List<Exception>();
        }

        public string Name
        {
            get { return Class.FullName + "." + Method.Name; }
        }

        public Type Class { get; private set; }
        public MethodInfo Method { get; private set; }
        public IReadOnlyList<Exception> Exceptions { get { return exceptions; } }

        internal void Fail(Exception reason)
        {
            exceptions.Add(reason);
        }
    }
}
