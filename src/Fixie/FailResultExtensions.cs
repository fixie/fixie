using System.IO;
using System.Linq;

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

        public static string PrimaryMessage(this FailResult failResult)
        {
            return failResult.Exceptions.First().Message;
        }

        public static string PrimaryTypeName(this FailResult failResult)
        {
            return failResult.Exceptions.First().GetType().FullName;
        }
    }
}