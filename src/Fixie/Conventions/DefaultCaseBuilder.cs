using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fixie.Conventions
{
    internal class DefaultCaseBuilder : ICaseBuilder
    {
        public Case Build(Type testClass, MethodInfo method, object[] parameters)
        {
            return new Case(testClass, method, parameters);
        }
    }
}
