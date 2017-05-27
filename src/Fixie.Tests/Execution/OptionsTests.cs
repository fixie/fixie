namespace Fixie.Tests.Execution
{
    using System;
    using Fixie.Cli;
    using Fixie.Execution;

    public class OptionsTests
    {
        public void DemandsAssemblyPathProvided()
        {
            var options = new Options(null);

            Action validate = options.Validate;

            validate.ShouldThrow<CommandLineException>(
                "Missing required test assembly path.");
        }

        public void DemandsAssemblyPathExistsOnDisk()
        {
            var options = new Options("foo.dll");

            Action validate = options.Validate;

            validate.ShouldThrow<CommandLineException>(
                "Specified test assembly does not exist: foo.dll");
        }

        public void DemandsAssemblyPathDirectoryContainsFixie()
        {
            var mscorlib = typeof(string).Assembly.Location;

            var options = new Options(mscorlib);

            Action validate = options.Validate;

            validate.ShouldThrow<CommandLineException>(
                $"Specified assembly {mscorlib} does not appear to be a test assembly. Ensure that it references Fixie.dll and try again.");
        }

        public void AcceptsExistingTestAssemblyPath()
        {
            var assemblyPath = typeof(OptionsTests).Assembly.Location;

            var options = new Options(assemblyPath);

            options.Validate();
        }

        public void DemandsValidReportFileNameWhenProvided()
        {
            var assemblyPath = typeof(OptionsTests).Assembly.Location;

            var options = new Options(assemblyPath);

            Action validate = options.Validate;

            options.Report = "Report.xml";
            validate();

            options.Report = "\t";
            validate.ShouldThrow<CommandLineException>(
                "Specified report name is invalid: \t");
        }
    }
}