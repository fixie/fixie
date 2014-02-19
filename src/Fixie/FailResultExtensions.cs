using System.Collections.Generic;
using System.IO;
using Fixie.Results;

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
            WriteCompoundStackTrace(writer, failResult.Exceptions);
        }

        static void WriteCompoundStackTrace(TextWriter console, IEnumerable<ExceptionInfo> exceptions)
        {
            bool isPrimaryException = true;

            foreach (var ex in exceptions)
            {
                if (isPrimaryException)
                {
                    console.WriteLine(ex.Message);
                    console.Write(ex.StackTrace);
                }
                else
                {
                    console.WriteLine();
                    console.WriteLine();
                    using (Foreground.DarkGray)
                        console.WriteLine("===== Secondary Exception: {0} =====", ex.Type);
                    console.WriteLine(ex.Message);
                    console.Write(ex.StackTrace);
                }

                var walk = ex;
                while (walk.InnerException != null)
                {
                    walk = walk.InnerException;
                    console.WriteLine();
                    console.WriteLine();
                    using (Foreground.DarkGray)
                        console.WriteLine("------- Inner Exception: {0} -------", walk.Type);
                    console.WriteLine(walk.Message);
                    console.Write(walk.StackTrace);
                }

                isPrimaryException = false;
            }
        }
    }
}