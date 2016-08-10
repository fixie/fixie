namespace Fixie.Runner
{
    using Execution;
    using Microsoft.Extensions.Testing.Abstractions;
    
    public class DesignTimeDiscoveryListener : Handler<MethodGroupDiscovered>
    {
        readonly IDesignTimeSink sink;

        public DesignTimeDiscoveryListener(IDesignTimeSink sink)
        {
            this.sink = sink;
        }

        public void Handle(MethodGroupDiscovered message)
        {
            var methodGroup = message.MethodGroup.FullName;

            sink.SendTestFound(new Test
            {
                FullyQualifiedName = methodGroup,
                DisplayName = methodGroup,

                //TODO: Discover source location.
                CodeFilePath = null,
                LineNumber = null
            });
        }
    }
}