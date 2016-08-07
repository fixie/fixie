namespace Fixie.ConsoleRunner.Reports
{
    using System;
    using System.IO;
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
        readonly Action<AssemblyReport> save;

        public ReportListener(Action<AssemblyReport> save)
        {
            this.save = save;
        }

        public ReportListener()
            : this(Save)
        {
        }

        public void Handle(AssemblyStarted message)
        {
            assembly = new AssemblyReport(message.Assembly.Location);
        }

        public void Handle(ClassStarted message)
        {
            currentClass = new ClassReport(message.TestClass);
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
            save(assembly);
            assembly = null;
        }

        static void Save(AssemblyReport assembly)
        {
            var format = new TXmlFormat();
            var xDocument = format.Transform(assembly);
            var folder = Path.GetDirectoryName(assembly.Location);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assembly.Location);
            var formatName = format.Name;
            var filePath = Path.Combine(folder, $"{fileNameWithoutExtension}.{formatName}.xml");
            xDocument.Save(filePath, SaveOptions.None);
        }
    }
}