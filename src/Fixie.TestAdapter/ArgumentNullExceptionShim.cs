#if NETCOREAPP3_1
namespace Fixie.TestAdapter
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    
    static class ArgumentNullException
    {
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (argument is null)
                throw new System.ArgumentNullException(paramName);
        }
    }
}
#endif