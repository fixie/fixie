namespace Fixie.Console
{
    public class Options
    {
        public Options(params string[] patterns)
        {
            Patterns = patterns;
        }

        public string[] Patterns { get; }

        string configuration;
        public string Configuration
        {
            get => configuration ?? "Debug";
            set => configuration = value;
        }

        public bool NoBuild { get; set; }
        public bool ShouldBuild => !NoBuild;
        public string Framework { get; set; }
        public string Report { get; set; }
        public bool? TeamCity { get; set; }
    }
}