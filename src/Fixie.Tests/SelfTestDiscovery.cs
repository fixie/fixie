using System.Reflection;

namespace Fixie.Tests;

public class SelfTestDiscovery : IDiscovery
{
    public IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses)
    {
        return concreteClasses
            .Where(x => x.IsNestedPrivate || x.IsNestedFamily)
            .Where(x => x.Name.EndsWith("TestClass"));
    }

    public virtual IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
        => publicMethods.OrderBy(x => x.Name, StringComparer.Ordinal);
}