using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ReportListener :
        LongLivedMarshalByRefObject,
        IHandler<CaseCompleted>
    {
        readonly AssemblyReport report;
        ClassReport currentClass;

        public ReportListener(AssemblyReport report)
        {
            this.report = report;
        }

        public void Handle(CaseCompleted message)
        {
            if (currentClass == null || currentClass.Name != message.MethodGroup.Class)
            {
                currentClass = new ClassReport(message.MethodGroup.Class);
                report.Add(currentClass);
            }

            currentClass.Add(message);
        }
    }
}