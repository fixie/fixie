namespace Fixie.Execution.Listeners
{
    using System;

    public class ReportListener :
        Handler<AssemblyStarted>,
        Handler<ClassStarted>,
        Handler<CaseCompleted>,
        Handler<ClassCompleted>,
        Handler<AssemblyCompleted>
    {
        Report report;
        ClassReport currentClass;
        readonly Action<Report> save;

        public ReportListener(Action<Report> save)
        {
            this.save = save;
        }

        public void Handle(AssemblyStarted message)
        {
            report = new Report(message.Assembly);
        }

        public void Handle(ClassStarted message)
        {
            currentClass = new ClassReport(message.Class);
            report.Add(currentClass);
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
            save(report);
            report = null;
        }
    }
}