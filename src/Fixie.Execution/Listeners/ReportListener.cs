namespace Fixie.Execution.Listeners
{
    using System;
    using System.Xml.Linq;

    public class ReportListener<TXmlFormat> :
        Handler<AssemblyStarted>,
        Handler<CaseCompleted>,
        Handler<AssemblyCompleted>
        where TXmlFormat : XmlFormat, new()
    {
        Report report;
        ClassReport currentClass;
        readonly XmlFormat format;
        readonly Action<XDocument> save;

        public ReportListener(Action<XDocument> save)
        {
            format = new TXmlFormat();
            this.save = save;
        }

        public ReportListener(string fileName)
            : this(xDocument => xDocument.Save(fileName, SaveOptions.None))
        {
        }

        public void Handle(AssemblyStarted message)
        {
            report = new Report(message.Location);
            currentClass = null;
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

        public void Handle(AssemblyCompleted message)
        {
            save(format.Transform(report));
            currentClass = null;
            report = null;
        }
    }
}