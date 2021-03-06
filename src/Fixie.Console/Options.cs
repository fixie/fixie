﻿namespace Fixie.Console
{
    using System.IO;

    public class Options
    {
        public Options(
            string? configuration,
            bool noBuild,
            string? framework,
            string? report,
            string? tests,
            params string[] projectPatterns)
        {
            ProjectPatterns = projectPatterns;
            Configuration = configuration ?? "Debug";
            NoBuild = noBuild;
            Framework = framework;
            Report = report;
            Tests = tests;
        }

        public string[] ProjectPatterns { get; }
        public string Configuration { get; }
        public bool NoBuild { get; }
        public bool ShouldBuild => !NoBuild;
        public string? Framework { get; }
        public string? Report { get; }
        public string? Tests { get; }

        public void Validate()
        {
            if (Report != null && Report.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new CommandLineException("Specified report name is invalid: " + Report);
        }
    }
}