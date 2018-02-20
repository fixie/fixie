namespace Fixie.Execution
{
    using System;

    public class AssertionLibraryFilter
    {
        [Obsolete]
        public string FilterStackTrace(Exception exception) => exception.StackTrace;
    }
}