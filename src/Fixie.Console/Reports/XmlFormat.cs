namespace Fixie.ConsoleRunner.Reports
{
    using System.Xml.Linq;

    public interface XmlFormat
    {
        string Name { get; }
        XDocument Transform(AssemblyReport assemblyReport);
    }
}