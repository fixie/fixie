using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie;

public sealed class DefaultDiscovery : IDiscovery
{
    public IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses)
        => concreteClasses.Where(x => x.Name.EndsWith("Tests"));

    public IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
        => publicMethods.Where(x => !x.IsStatic);
}