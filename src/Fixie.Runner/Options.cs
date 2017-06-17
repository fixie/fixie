namespace Fixie.Runner
{
    public class Options
    {
        string configuration;
        public string Configuration
        {
            get => configuration ?? "Debug";
            set => configuration = value;
        }

        public bool NoBuild { get; set; }
        public string Framework { get; set; }
        public bool x86 { get; set; }
        public string Report { get; set; }
        public bool? TeamCity { get; set; }
    }
}