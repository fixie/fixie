using System.IO;

namespace Fixie
{
    public static class FailResultExtensions
    {
        public static string CompoundStackTrace(this FailResult failResult)
        {
            using (var writer = new StringWriter())
            {
                WriteCompoundStackTraceTo(failResult, writer);
                return writer.ToString();
            }
        }

        public static void WriteCompoundStackTraceTo(this FailResult failResult, TextWriter writer)
        {
            writer.WriteCompoundStackTrace(failResult.Exceptions);
        }
    }
}