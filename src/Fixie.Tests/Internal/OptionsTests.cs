namespace Fixie.Tests.Internal
{
    using System;
    using Assertions;
    using Fixie.Cli;
    using Fixie.Internal;

    public class OptionsTests
    {
        public void DemandsValidReportFileNameWhenProvided()
        {
            Action noReport = new Options(report: null).Validate;
            noReport();

            Action validReport = new Options(report: "Report.xml").Validate;
            validReport();

            Action invalidReport = new Options(report: "\0").Validate;
            invalidReport.ShouldThrow<CommandLineException>(
                "Specified report name is invalid: \0");
        }
    }
}