namespace Fixie.Tests.Console
{
    using System;
    using Assertions;
    using Fixie.Console;

    public class OptionsTests
    {
        public void DemandsValidReportFileNameWhenProvided()
        {
            Action noReport = new Options(null, false, null, report: null).Validate;
            noReport();

            Action validReport = new Options(null, false, null, report: "Report.xml").Validate;
            validReport();

            Action invalidReport = new Options(null, false, null, report: "\0").Validate;
            invalidReport.ShouldThrow<CommandLineException>(
                "Specified report name is invalid: \0");
        }
    }
}