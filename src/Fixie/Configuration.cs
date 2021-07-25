namespace Fixie
{
    public class Configuration
    {
        public ConventionCollection Conventions { get; } = new ConventionCollection();
        public ReportCollection Reports { get; } = new ReportCollection();
    }
}