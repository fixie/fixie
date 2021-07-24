namespace Fixie
{
    public abstract class Configuration
    {
        public ConventionCollection Conventions { get; } = new ConventionCollection();
    }
}