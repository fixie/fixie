using Fixie.Execution;

namespace Fixie.ConsoleRunner
{
    public class ReportListener :
        LongLivedMarshalByRefObject,
        IHandler<AssemblyStarted>,
        IHandler<CaseResult>,
        IHandler<AssemblyCompleted>
    {
        readonly Report result;
        AssemblyReport currentAssembly;
        ClassReport currentClass;

        public ReportListener(Report result)
        {
            this.result = result;
            currentAssembly = null;
            currentClass = null;
        }

        public void Handle(AssemblyStarted message)
        {
            currentAssembly = new AssemblyReport(message.Location);
            currentClass = null;
            result.Add(currentAssembly);
        }

        public void Handle(CaseResult message)
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
            currentAssembly = null;
            currentClass = null;
        }
    }
}