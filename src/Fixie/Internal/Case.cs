namespace Fixie.Internal
{
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// A test case being executed, representing a single call to a test method.
    /// </summary>
    class Case
    {
        readonly object?[] parameters;

        public Case(MethodInfo method, object?[] parameters)
        {
            this.parameters = parameters;
            ResolvedMethod = method.TryResolveTypeArguments(parameters);
        }

        /// <summary>
        /// Gets the method that defines this test case.
        /// </summary>
        public MethodInfo ResolvedMethod { get; }

        public async Task RunAsync(object? instance)
            => await ResolvedMethod.RunTestMethodAsync(instance, parameters);
    }
}