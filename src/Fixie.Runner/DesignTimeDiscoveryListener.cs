namespace Fixie.Runner
{
    using System;
    using Contracts;
    using Execution;

    public class DesignTimeDiscoveryListener : Handler<MethodGroupDiscovered>
    {
        readonly IDesignTimeSink sink;
        readonly SourceLocationProvider sourceLocationProvider;

        public DesignTimeDiscoveryListener(IDesignTimeSink sink, string assemblyPath)
        {
            this.sink = sink;
            sourceLocationProvider = new SourceLocationProvider(assemblyPath);
        }

        public void Handle(MethodGroupDiscovered message)
        {
            var methodGroup = message.MethodGroup;

            var test = new Test
            {
                FullyQualifiedName = methodGroup.FullName,
                DisplayName = methodGroup.FullName,
            };

            try
            {
                SourceLocation sourceLocation;
                if (sourceLocationProvider.TryGetSourceLocation(methodGroup, out sourceLocation))
                {
                    test.CodeFilePath = sourceLocation.CodeFilePath;
                    test.LineNumber = sourceLocation.LineNumber;
                }
            }
            catch (Exception exception)
            {
                sink.Log(exception.ToString());
            }

            sink.SendTestFound(test);
        }
    }
}