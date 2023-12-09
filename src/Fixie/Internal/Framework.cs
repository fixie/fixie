namespace Fixie.Internal;

using System.Reflection;

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