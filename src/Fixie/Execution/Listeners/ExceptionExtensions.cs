namespace Fixie.Execution.Listeners
{
    using System;
    using System.IO;

    static class ExceptionExtensions
    {
        public static string CompoundStackTrace(this Exception exception)
        {
            using (var console = new StringWriter())
            {
                var ex = exception;

                console.Write(ex.StackTrace);

                var walk = ex;
                while (walk.InnerException != null)
                {
                    walk = walk.InnerException;
                    console.WriteLine();
                    console.WriteLine();
                    console.WriteLine($"------- Inner Exception: {walk.TypeName()} -------");
                    console.WriteLine(walk.Message);
                    console.Write(walk.StackTrace);
                }

                return console.ToString();
            }
        }
    }
}