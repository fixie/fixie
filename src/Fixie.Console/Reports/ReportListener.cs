namespace Fixie.ConsoleRunner.Reports
{
    using System;
    using System.Xml.Linq;
    using Execution;

    public class ReportListener<TXmlFormat> :
        Handler<AssemblyStarted>,
        Handler<ClassStarted>,
        Handler<CaseCompleted>,
        Handler<ClassCompleted>,
        Handler<AssemblyCompleted>
        where TXmlFormat : XmlFormat, new()
    {
        Report report;
        AssemblyReport currentAssembly;
        ClassReport currentClass;
        private readonly XmlFormat format;
        readonly Action<XDocument> save;

        public ReportListener(Action<XDocument> save)
        {
            report = null;
            format = new TXmlFormat();
            this.save = save;
        }

        public ReportListener(string fileName)
            : this(xDocument => xDocument.Save(fileName, SaveOptions.None))
        {
        }

        public void Handle(AssemblyStarted message)
        {
            currentAssembly = new AssemblyReport(message.Location);
            report = new Report(currentAssembly);
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

            save(format.Transform(report));

            report = null;
        }
    }
}