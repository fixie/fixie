using System.Reflection;

namespace Fixie.Internal;

static class Framework
{
    public static string Version
        => "Fixie " + ProductVersion();

    static string ProductVersion()
        => typeof(Framework)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;
}