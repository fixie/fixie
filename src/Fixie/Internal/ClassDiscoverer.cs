using System.Runtime.CompilerServices;

namespace Fixie.Internal;

class ClassDiscoverer
{
    readonly IDiscovery discovery;

    public ClassDiscoverer(IDiscovery discovery)
        => this.discovery = discovery;

    public IReadOnlyList<Type> TestClasses(IEnumerable<Type> candidates)
    {
        try
        {
            return discovery.TestClasses(candidates.Where(IsApplicable)).ToList();
        }
        catch (Exception exception)
        {
            throw new Exception(
                "Exception thrown during test class discovery. " +
                "Check the inner exception for more details.", exception);
        }
    }

    static bool IsApplicable(Type candidate)
    {
        return ConcreteClasses(candidate) &&
               NonCompilerGeneratedClasses(candidate);
    }

    static bool ConcreteClasses(Type type)
        => type.IsClass && (!type.IsAbstract || type.IsStatic());

    static bool NonCompilerGeneratedClasses(Type type)
        => !type.Has<CompilerGeneratedAttribute>();
}