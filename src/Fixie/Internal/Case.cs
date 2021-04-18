namespace Fixie.Internal
{
    using System.Reflection;

    /// <summary>
    /// A test case being executed, representing a single call to a test method.
    /// </summary>
    class Case
    {
        public Case(MethodInfo method, object?[] parameters)
        {
            ResolvedMethod = method.TryResolveTypeArguments(parameters);
        }

        /// <summary>
        /// Gets the method that defines this test case.
        /// </summary>
        public MethodInfo ResolvedMethod { get; }
    }
}