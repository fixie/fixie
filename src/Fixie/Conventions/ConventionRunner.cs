using System;
using Fixie.Results;

namespace Fixie.Conventions
{
    public class ConventionRunner
    {
        public ConventionResult Run(Convention convention, Listener listener, params Type[] candidateTypes)
        {
            return convention.Execute(listener, candidateTypes);
        }
    }
}