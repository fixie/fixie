namespace Fixie.Internal.Listeners
{
    using System;
    using System.Text;

    static class ExceptionExtensions
    {
        public static string LiterateStackTrace(this Exception exception)
        {
            var console = new StringBuilder();

            console.Append(exception.StackTrace);

            var walk = exception;
            while (walk.InnerException != null)
            {
                walk = walk.InnerException;
                console.AppendLine();
                console.AppendLine();
                console.AppendLine($"------- Inner Exception: {walk.GetType().FullName} -------");
                console.AppendLine(walk.Message);
                console.Append(walk.StackTrace);
            }

            return console.ToString();
        }
    }
}