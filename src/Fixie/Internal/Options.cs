namespace Fixie.Internal
{
    using System.IO;
    using Cli;

    class Options
    {
        public Options(string? report)
        {
            Report = report;
        }

        public string? Report { get; }

        public void Validate()
        {
            if (Report != null && Report.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new CommandLineException("Specified report name is invalid: " + Report);
        }
    }
}