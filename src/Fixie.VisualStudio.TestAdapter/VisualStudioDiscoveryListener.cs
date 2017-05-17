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
            sourceLocationProvider = new SourceLocationProvider(assemblyPath);
        }

        public void Handle(MethodDiscovered message)
        {
            var methodGroup = new MethodGroup(message.Class, message.Method);

            var fullyQualifiedName = methodGroup.FullName;
            var displayName = methodGroup.FullName;
            SourceLocation sourceLocation = null;

            try
            {
                sourceLocationProvider.TryGetSourceLocation(methodGroup, out sourceLocation);
            }
            catch (Exception exception)
            {
                discoveryRecorder.Error(exception.ToString());
            }

            if (sourceLocation != null)
                discoveryRecorder.SendTestFound(
                    fullyQualifiedName,
                    displayName,
                    sourceLocation.CodeFilePath,
                    sourceLocation.LineNumber);
            else
                discoveryRecorder.SendTestFound(fullyQualifiedName, displayName);
        }
    }
}