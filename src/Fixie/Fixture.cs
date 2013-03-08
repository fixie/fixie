using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class Fixture
    {
        readonly Type fixtureClass;

        public Fixture(Type fixtureClass)
        {
            this.fixtureClass = fixtureClass;
        }

        public string Name
        {
            get { return fixtureClass.Name; }
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
                                   .Select(method => new Case(fixtureClass, method));
            }
        }
    }
}