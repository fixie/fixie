namespace Fixie.Tests.Console
{
    using System;
    using Assertions;
    using Fixie.Console;

    public class OptionsTests
    {
        public void DemandsValidReportFileNameWhenProvided()
        {
            Action noReport = new Options(null, false, null, report: null, tests: null).Validate;
            noReport();

            Action validReport = new Options(null, false, null, report: "Report.xml", tests: null).Validate;
            validReport();

            Action invalidReport = new Options(null, false, null, report: "\0", tests: null).Validate;
            invalidReport.ShouldThrow<CommandLineException>(
                "Specified report name is invalid: \0");
        }
    }
}