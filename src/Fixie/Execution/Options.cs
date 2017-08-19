namespace Fixie.Execution
{
    using System.IO;
    using Cli;

    class Options
    {
        public Options(params string[] patterns)
        {
            Patterns = patterns;
        }

        public string[] Patterns { get; }
        public string Report { get; set; }
        public bool? TeamCity { get; set; }

        public void Validate()
        {
            if (Report != null && Report.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new CommandLineException("Specified report name is invalid: " + Report);
        }
    }
}