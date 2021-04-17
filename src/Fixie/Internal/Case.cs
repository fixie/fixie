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
            Method = method.TryResolveTypeArguments(parameters);
            Name = CaseNameBuilder.GetName(Method, parameters);
        }

        /// <summary>
        /// Gets the name of the test case, including any input parameters.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the method that defines this test case.
        /// </summary>
        public MethodInfo Method { get; }

        public async Task RunAsync(object? instance)
            => await Method.RunTestMethodAsync(instance, parameters);
    }
}