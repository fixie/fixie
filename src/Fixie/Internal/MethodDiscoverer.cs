namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class MethodDiscoverer
    {
        readonly Discovery discovery;

        public MethodDiscoverer(Discovery discovery)
            => this.discovery = discovery;

        public IReadOnlyList<MethodInfo> TestMethods(Type testClass)
        {
            try
            {
                return discovery.TestMethods(
                        testClass
                            .GetMethods()
                            .Where(method => method.DeclaringType != typeof(object)))
                    .ToList();
            }
            catch (Exception exception)
            {
                throw new Exception(
                    "Exception thrown during test method discovery. " +
                    "Check the inner exception for more details.", exception);
            }
        }
    }
}