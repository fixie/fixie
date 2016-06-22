namespace Fixie.ConsoleRunner.Reports
{
    using System.Xml.Linq;

    public interface XmlFormat
    {
        XDocument Transform(Report report);
    }
}