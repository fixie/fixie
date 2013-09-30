using System;
using System.Linq;
using System.Reflection;

namespace Fixie.Conventions
{
    public interface ICaseBuilder
    {
        Case Build(Type type, MethodInfo method, object[] parameters);
    }
}
