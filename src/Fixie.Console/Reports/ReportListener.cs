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
        AssemblyReport assembly;
        ClassReport currentClass;
        private readonly XmlFormat format;
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
            assembly = new AssemblyReport(message.Location);
        }

        public void Handle(ClassStarted message)
        {
            currentClass = new ClassReport(message.FullName);
            assembly.Add(currentClass);
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
            save(format.Transform(assembly));

            assembly = null;
        }
    }
}