namespace Fixie.Console
{
    public class Options
    {
        public Options(
            string configuration,
            bool noBuild,
            string framework,
            string report,
            bool? teamCity,
            params string[] patterns)
        {
            Configuration = configuration ?? "Debug";
            NoBuild = noBuild;
            Framework = framework;
            Report = report;
            TeamCity = teamCity;
            Patterns = patterns;
        }

        public string Configuration { get; }
        public bool NoBuild { get; }
        public bool ShouldBuild => !NoBuild;
        public string Framework { get; }
        public string Report { get; }
        public bool? TeamCity { get; }
        public string[] Patterns { get; }
    }
}