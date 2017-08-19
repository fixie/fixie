namespace Fixie.Tests.Execution
{
    using System;
    using Fixie.Cli;
    using Fixie.Execution;

    public class OptionsTests
    {
        public void DemandsValidReportFileNameWhenProvided()
        {
            var options = new Options();

            Action validate = options.Validate;

            validate();

            options.Report = "Report.xml";
            validate();

            options.Report = "\t";
            validate.ShouldThrow<CommandLineException>(
                "Specified report name is invalid: \t");
        }
    }
}