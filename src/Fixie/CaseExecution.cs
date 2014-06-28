using System;
using System.Collections.Generic;
using Fixie.Behaviors;

namespace Fixie
{
    public class CaseExecution : BehaviorContext
    {
        readonly List<Exception> exceptions;

        public CaseExecution(Case @case)
        {
            Case = @case;
            exceptions = new List<Exception>();
        }

        public Case Case { get; private set; }

        public object Instance { get; internal set; }
        
        public TimeSpan Duration { get; internal set; }

        public string Output { get; internal set; }

        public object Result { get; internal set; }

        public IReadOnlyList<Exception> Exceptions { get { return exceptions; } }

        public void Fail(Exception reason)
        {
            var wrapped = reason as PreservedException;

            if (wrapped != null)
                exceptions.Add(wrapped.OriginalException);
            else
                exceptions.Add(reason);
        }

        public void Pass()
        {
            exceptions.Clear();
        }
    }
}