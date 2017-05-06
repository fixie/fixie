namespace Fixie.VisualStudio.TestAdapter
{
    using System;
    using Execution;

    public class VisualStudioDiscoveryListener : Handler<MethodDiscovered>
    {
        readonly IDiscoveryRecorder discoveryRecorder;
        readonly SourceLocationProvider sourceLocationProvider;

        public VisualStudioDiscoveryListener(IDiscoveryRecorder discoveryRecorder, string assemblyPath)
        {
            this.discoveryRecorder = discoveryRecorder;
            this.sourceLocationProvider = new SourceLocationProvider(assemblyPath);
        }

        public void Handle(MethodDiscovered message)
        {
            var methodGroup = new MethodGroup(message.Class, message.Method);

            var fullyQualifiedName = methodGroup.FullName;
            var displayName = methodGroup.FullName;

            try
            {
                SourceLocation sourceLocation;
                if (sourceLocationProvider.TryGetSourceLocation(methodGroup, out sourceLocation))
                    discoveryRecorder.SendTestFound(
                        fullyQualifiedName,
                        displayName,
                        sourceLocation.CodeFilePath,
                        sourceLocation.LineNumber);
            }
            catch (Exception exception)
            {
                discoveryRecorder.Error(exception.ToString());

                discoveryRecorder.SendTestFound(fullyQualifiedName, displayName);
            }
        }
    }
}