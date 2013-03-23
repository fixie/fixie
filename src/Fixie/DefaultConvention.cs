using System;
using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class DefaultConvention : Convention
    {
        protected override bool ClassIsFixture(Type concreteClass)
        {
            return concreteClass.Name.EndsWith("Tests") && concreteClass.GetConstructor(Type.EmptyTypes) != null;
        }

        protected override MethodInfo[] QueryCaseMethods(Type fixtureClass)
        {
            return fixtureClass.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(method =>
                                      method.ReturnType == typeof(void) &&
                                      method.GetParameters().Length == 0).ToArray();
        }
    }
}