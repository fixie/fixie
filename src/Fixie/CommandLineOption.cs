namespace Fixie
{
    public class CommandLineOption
    {
        public const string Include = "Include";
        public const string Exclude = "Exclude";
        public const string NUnitXml = "NUnitXml";
        public const string XUnitXml = "XUnitXml";
        public const string TeamCity = "TeamCity";
        public const string Parameter = "Parameter";

        public static string[] GetAll()
        {
            return new[]
            {
                Include,
                Exclude,
                NUnitXml,
                XUnitXml,
                TeamCity,
                Parameter
            };
        }
    }
}