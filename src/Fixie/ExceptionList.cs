using System;
using System.Collections.Generic;
using System.Linq;

namespace Fixie
{
    public class ExceptionList
    {
        readonly List<Exception> exceptions = new List<Exception>();

        public void Add(Exception exception)
        {
            exceptions.Add(exception);
        }

        public void Add(ExceptionList exceptionList)
        {
            exceptions.AddRange(exceptionList.exceptions);
        }

        public int Count
        {
            get { return exceptions.Count; }
        }

        public bool Any()
        {
            return exceptions.Any();
        }

        public Exception[] ToArray()
        {
            return exceptions.ToArray();
        }
    }
}