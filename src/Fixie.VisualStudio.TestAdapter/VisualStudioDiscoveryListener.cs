namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Execution;

    public class VisualStudioDiscoveryListener : Handler<MethodDiscovered>
    {
        readonly DiscoveryRecorder discoveryRecorder;
        readonly SourceLocationProvider sourceLocationProvider;

        public VisualStudioDiscoveryListener(DiscoveryRecorder discoveryRecorder, string assemblyPath)
        {
            this.discoveryRecorder = discoveryRecorder;
            this.sourceLocationProvider = new SourceLocationProvider(assemblyPath);
        }

        public void Handle(MethodDiscovered message)
        {
            var methodGroup = new MethodGroup(message.Method);

            var test = new Test
            {
                FullyQualifiedName = methodGroup.FullName,
                DisplayName = methodGroup.FullName
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
                discoveryRecorder.Error(exception.ToString());
            }

            discoveryRecorder.SendTestFound(test);
        }
    }
}