namespace Fixie.Internal
{
    class DefaultConfiguration : Configuration
    {
        public DefaultConfiguration()
            => Conventions.Add<DefaultDiscovery, DefaultExecution>();
    }
}