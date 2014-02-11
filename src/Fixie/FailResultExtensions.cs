using System;
using System.Collections.Generic;
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
            WriteCompoundStackTrace(writer, failResult.Exceptions);
        }

        public static string PrimaryExceptionMessage(this FailResult failResult)
        {
            return failResult.Exceptions.First().Message;
        }

        public static string PrimaryExceptionTypeName(this FailResult failResult)
        {
            return failResult.Exceptions.First().GetType().FullName;
        }

        static void WriteCompoundStackTrace(TextWriter console, IEnumerable<Exception> exceptions)
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
                        console.WriteLine("===== Secondary Exception: {0} =====", ex.GetType().FullName);
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
                        console.WriteLine("------- Inner Exception: {0} -------", walk.GetType().FullName);
                    console.WriteLine(walk.Message);
                    console.Write(walk.StackTrace);
                }

                isPrimaryException = false;
            }
        }
    }
}