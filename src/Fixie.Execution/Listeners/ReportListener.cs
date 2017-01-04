namespace Fixie.Execution.Listeners
{
    public class ReportListener :
        Handler<AssemblyStarted>,
        Handler<CaseCompleted>,
        Handler<AssemblyCompleted>
    {
        ClassReport currentClass;

        public Report Report { get; private set; }

        public void Handle(AssemblyStarted message)
        {
            currentClass = null;
            Report = new Report(message.Location);
        }

        public void Handle(CaseCompleted message)
        {
            if (currentClass == null || currentClass.Name != message.MethodGroup.Class)
            {
                currentClass = new ClassReport(message.MethodGroup.Class);
                Report.Add(currentClass);
            }

            currentClass.Add(message);
        }

        public void Handle(AssemblyCompleted message)
        {
            currentClass = null;
        }
    }
}