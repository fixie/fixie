namespace Fixie
{
    using Internal;

    public abstract class Configuration
    {
        internal Convention Convention { get; private set; }

        protected Configuration()
        {
            Convention = new Convention(new DefaultDiscovery(), new DefaultExecution());
        }

        public void Use<TDiscovery, TExecution>()
            where TDiscovery : IDiscovery, new()
            where TExecution : IExecution, new()
        {
            Use(new TDiscovery(), new TExecution());
        }

        public void Use(IDiscovery discovery, IExecution execution)
        {
            Convention = new Convention(discovery, execution);
        }
    }
}