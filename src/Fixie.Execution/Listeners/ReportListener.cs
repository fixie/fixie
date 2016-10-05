namespace Fixie.Execution.Listeners
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
            assembly = new AssemblyReport(message.Assembly);
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

        static void Save(AssemblyReport assemblyReport)
        {
            var format = new TXmlFormat();
            var xDocument = format.Transform(assemblyReport);
            var folder = Path.GetDirectoryName(assemblyReport.Assembly.Location);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assemblyReport.Assembly.Location);
            var formatName = format.Name;
            var filePath = Path.Combine(folder, $"{fileNameWithoutExtension}.{formatName}.xml");

            using (var stream = new FileStream(filePath, FileMode.Create))
            using (var writer = new StreamWriter(stream))
                xDocument.Save(writer, SaveOptions.None);
        }
    }
}