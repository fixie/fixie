namespace Fixie.Tests.Execution
{
    using System;
    using Fixie.Cli;
    using Fixie.Execution;

    public class OptionsTests
    {
        public void DemandsValidReportFileNameWhenProvided()
        {
            Action noReport = new Options(report: null, teamCity: null, patterns: null).Validate;
            noReport();

            Action validReport = new Options(report: "Report.xml", teamCity: null, patterns: null).Validate;
            validReport();

            Action invalidReport = new Options(report: "\t", teamCity: null, patterns: null).Validate;
            invalidReport.ShouldThrow<CommandLineException>(
                "Specified report name is invalid: \t");
        }
    }
}