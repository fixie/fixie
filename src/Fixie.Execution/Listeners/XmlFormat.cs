namespace Fixie.Execution.Listeners
{
    using System.Xml.Linq;

    public interface XmlFormat
    {
        XDocument Transform(Report report);
    }
}