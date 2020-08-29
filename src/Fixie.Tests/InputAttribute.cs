namespace Fixie.Tests
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class InputAttribute : Attribute
    {
        public InputAttribute(int order, params object?[] parameters)
        {
            Order = order;
            Parameters = parameters;
        }

        public int Order { get; }
        public object?[] Parameters { get; }
    }
}
