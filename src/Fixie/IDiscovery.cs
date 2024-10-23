using System.Reflection;

namespace Fixie;

public interface IDiscovery
{
    /// <summary>
    /// Filters a set of candidate classes to those which are to be treated as test classes.
    /// </summary>
    IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses);

    /// <summary>
    /// Filters a set of candidate methods to those which are to be treated as test methods.
    /// </summary>
    IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods);
}

/// <summary>
/// Discover test methods as the public instance methods on classes where the class name ends with "Tests".
/// </summary>
public sealed class DefaultDiscovery : IDiscovery
{
    public IEnumerable<Type> TestClasses(IEnumerable<Type> concreteClasses)
        => concreteClasses.Where(x => x.Name.EndsWith("Tests"));

    public IEnumerable<MethodInfo> TestMethods(IEnumerable<MethodInfo> publicMethods)
        => publicMethods.Where(x => !x.IsStatic);
}