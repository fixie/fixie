namespace Fixie.Execution.Listeners
{
    using System;
    using System.IO;
    using System.Xml.Linq;

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

        public ReportListener(string path)
            : this(report => Save(report, path))
        {
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

        static void Save(Report report, string path)
        {
            var format = new XUnitXml();
            var xDocument = format.Transform(report);

            var directory = Path.GetDirectoryName(path);

            if (String.IsNullOrEmpty(directory))
                return;

            Directory.CreateDirectory(directory);

            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = new StreamWriter(stream))
                xDocument.Save(writer, SaveOptions.None);
        }
    }
}