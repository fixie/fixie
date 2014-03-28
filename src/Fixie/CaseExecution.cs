﻿using System;
using System.Collections.Generic;

namespace Fixie
{
    public class CaseExecution
    {
        readonly List<Exception> exceptions;

        public CaseExecution(Case @case)
        {
            Case = @case;
            exceptions = new List<Exception>();
        }

        public Case Case { get; private set; }
        
        public TimeSpan Duration { get; set; }

        public string Output { get; set; }

        public IEnumerable<Exception> Exceptions { get { return exceptions; } }

        public void Fail(Exception reason)
        {
            var wrapped = reason as PreservedException;

            if (wrapped != null)
                exceptions.Add(wrapped.OriginalException);
            else
                exceptions.Add(reason);
        }
    }
}