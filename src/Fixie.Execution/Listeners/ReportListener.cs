namespace Fixie.Execution.Listeners
{
    using System;
    using System.IO;
    using System.Xml.Linq;

    public class ReportListener<TXmlFormat> :
        Handler<AssemblyStarted>,
        Handler<CaseCompleted>,
        Handler<AssemblyCompleted>
        where TXmlFormat : XmlFormat, new()
    {
        Report report;
        ClassReport currentClass;
        readonly Action<Report> save;

        public ReportListener(Action<Report> save)
        {
            this.save = save;
        }

        public ReportListener()
            : this(Save)
        {
        }

        public void Handle(AssemblyStarted message)
        {
            report = new Report(message.Assembly.Location);
            currentClass = null;
        }

        public void Handle(CaseCompleted message)
        {
            if (currentClass == null || currentClass.Name != message.Class.FullName)
            {
                currentClass = new ClassReport(message.Class.FullName);
                report.Add(currentClass);
            }

            currentClass.Add(message);
        }

        public void Handle(AssemblyCompleted message)
        {
            save(report);
            currentClass = null;
            report = null;
        }

        static void Save(Report report)
        {
            var format = new TXmlFormat();
            var xDocument = format.Transform(report);
            var filePath = Path.GetFullPath(report.Location) + ".xml";
            xDocument.Save(filePath, SaveOptions.None);
        }
    }
}