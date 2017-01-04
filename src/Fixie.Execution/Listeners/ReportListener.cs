namespace Fixie.Execution.Listeners
{
    public class ReportListener :
        Handler<AssemblyStarted>,
        Handler<CaseCompleted>,
        Handler<AssemblyCompleted>
    {
        AssemblyReport currentAssembly;
        ClassReport currentClass;

        public ReportListener()
        {
            Report = new ExecutionReport();
        }

        public ExecutionReport Report { get; }

        public void Handle(AssemblyStarted message)
        {
            currentAssembly = new AssemblyReport(message.Location);
        }

        public void Handle(CaseCompleted message)
        {
            if (currentClass == null || currentClass.Name != message.MethodGroup.Class)
            {
                currentClass = new ClassReport(message.MethodGroup.Class);
                currentAssembly.Add(currentClass);
            }

            currentClass.Add(message);
        }

        public void Handle(AssemblyCompleted message)
        {
            Report.Add(currentAssembly);

            currentAssembly = null;
        }
    }
}