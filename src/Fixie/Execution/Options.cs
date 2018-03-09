namespace Fixie.Execution
{
    using System.IO;
    using Cli;

    class Options
    {
        public Options(
            string report,
            bool? teamCity)
        {
            Report = report;
            TeamCity = teamCity;
        }

        public string Report { get; }
        public bool? TeamCity { get; }

        public void Validate()
        {
            if (Report != null && Report.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new CommandLineException("Specified report name is invalid: " + Report);
        }
    }
}