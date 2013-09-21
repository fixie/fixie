using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fixie
{
    public class Case
    {
        readonly object[] parameters;
        readonly List<Exception> exceptions;
        readonly Stopwatch stopwatch;

        public Case(Type testClass, MethodInfo caseMethod, params object[] parameters)
        {
            this.parameters = parameters != null && parameters.Length == 0 ? null : parameters;
            Class = testClass;
            Method = caseMethod;
            exceptions = new List<Exception>();
            stopwatch = new Stopwatch();
            Name = GetName();
        }

        string GetName()
        {
            var name = Class.FullName + "." + Method.Name;

            if (parameters != null && parameters.Length > 0)
                name = string.Format("{0}({1})", name, string.Join(", ", parameters.Select(x => x.ToDisplayString())));

            return name;
        }

        public string Name { get; private set; }
        public Type Class { get; private set; }
        public MethodInfo Method { get; private set; }
        public IReadOnlyList<Exception> Exceptions { get { return exceptions; } }

        public void Execute(object instance)
        {
            try
            {
                bool isDeclaredAsync = Method.IsAsync();

                if (isDeclaredAsync && Method.IsVoid())
                    ThrowForUnsupportedAsyncVoid();

                object result;
                try
                {
                    result = Method.Invoke(instance, parameters);
                }
                catch (TargetInvocationException exception)
                {
                    throw new PreservedException(exception.InnerException);
                }

                if (isDeclaredAsync)
                {
                    var task = (Task)result;
                    try
                    {
                        task.Wait();
                    }
                    catch (AggregateException exception)
                    {
                        throw new PreservedException(exception.InnerExceptions.First());
                    }
                }
            }
            catch (Exception exception)
            {
                Fail(exception);
            }
        }

        static void ThrowForUnsupportedAsyncVoid()
        {
            throw new NotSupportedException(
                "Async void methods are not supported. Declare async methods with a " +
                "return type of Task to ensure the task actually runs to completion.");
        }

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
