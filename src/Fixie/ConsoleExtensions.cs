using System;
using System.Collections.Generic;
using System.IO;

namespace Fixie
{
    public static class ConsoleExtensions
    {
        public static void WriteCompoundStackTrace(this TextWriter console, IEnumerable<Exception> exceptions)
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