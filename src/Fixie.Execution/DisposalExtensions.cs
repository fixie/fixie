using System;
using System.Linq;

namespace Fixie.Execution
{
    using System.Reflection;

    public static class DisposalExtensions
    {
        public static bool IsDisposable(this Type type)
            => type.GetInterfaces().Any(interfaceType => interfaceType == typeof(IDisposable));

        public static bool HasDisposeSignature(this MethodInfo method)
            => method.Name == "Dispose" && method.IsVoid() && method.GetParameters().Length == 0;
    }
}