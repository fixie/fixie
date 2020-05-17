namespace Fixie.Console
{
    public class Options
    {
        public Options(
            string? configuration,
            bool noBuild,
            string? framework,
            string? report)
        {
            Configuration = configuration ?? "Debug";
            NoBuild = noBuild;
            Framework = framework;
            Report = report;
        }

        public string Configuration { get; }
        public bool NoBuild { get; }
        public bool ShouldBuild => !NoBuild;
        public string? Framework { get; }
        public string? Report { get; }
    }
}