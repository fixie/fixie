namespace Fixie.ConsoleRunner.Reports
{
    public class Report
    {
        public Report(AssemblyReport assembly)
        {
            Assembly = assembly;
        }

        public AssemblyReport Assembly { get; }

        public int Passed => Assembly.Passed;
        public int Failed => Assembly.Failed;
        public int Skipped => Assembly.Skipped;
        public int Total => Passed + Failed + Skipped;
    }
}