using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class ClassFixture : Fixture
    {
        readonly Type fixtureClass;

        public ClassFixture(Type fixtureClass)
        {
            this.fixtureClass = fixtureClass;
        }

        public string Name
        {
            get { return fixtureClass.FullName; }
        }

        public IEnumerable<Case> Cases
        {
            get
            {
                return fixtureClass.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                   .Where(method =>
                                          method.ReturnType == typeof(void) &&
                                          method.GetParameters().Length == 0 &&
                                          method.DeclaringType != typeof(object))
                                   .Select(method => new MethodCase(fixtureClass, method));
            }
        }
    }
}