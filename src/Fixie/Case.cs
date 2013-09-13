using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Fixie
{
    public class Case
    {
        readonly List<Exception> exceptions;
        readonly Stopwatch stopwatch;

        public Case(Type testClass, MethodInfo caseMethod)
        {
            Class = testClass;
            Method = caseMethod;
            exceptions = new List<Exception>();
            stopwatch = new Stopwatch();
        }

        public string Name
        {
            get { return Class.FullName + "." + Method.Name; }
        }

        public Type Class { get; private set; }
        public MethodInfo Method { get; private set; }
        public IReadOnlyList<Exception> Exceptions { get { return exceptions; } }

        public void StartTimer()
        {
            stopwatch.Start();
        }

        public void StopTimer()
        {
            stopwatch.Stop();
        }

        public TimeSpan Duration
        {
            get { return stopwatch.Elapsed; }
        }

        internal void Fail(Exception reason)
        {
            var wrapped = reason as PreservedException;

            if (wrapped != null)
                exceptions.Add(wrapped.OriginalException);
            else
                exceptions.Add(reason);
        }
    }
}
