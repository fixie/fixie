namespace Fixie.ConsoleRunner.Reports
{
    using Execution;

    public class ReportListener :
        Handler<AssemblyStarted>,
        Handler<ClassStarted>,
        Handler<CaseCompleted>,
        Handler<ClassCompleted>,
        Handler<AssemblyCompleted>
    {
        AssemblyReport currentAssembly;
        ClassReport currentClass;

        public ReportListener()
        {
            Report = new Report();
        }

        public Report Report { get; }

        public void Handle(AssemblyStarted message)
        {
            currentAssembly = new AssemblyReport(message.Location);
            Report.Add(currentAssembly);
        }

        public void Handle(ClassStarted message)
        {
            currentClass = new ClassReport(message.FullName);
            currentAssembly.Add(currentClass);
        }

        public void Handle(CaseCompleted message)
        {
            currentClass.Add(message);
        }

        public void Handle(ClassCompleted message)
        {
            currentClass = null;
        }

        public void Handle(AssemblyCompleted message)
        {
            currentAssembly = null;
        }
    }
}