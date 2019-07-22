namespace Fixie.Internal
{
    using System;
    using System.Reflection;

    public class CaseStarted : Message
    {
        public CaseStarted(Case @case)
        {
            Class = @case.Class;
            Method = @case.Method;
            Name = @case.Name;
        }

        public Type Class { get; }
        public MethodInfo Method { get; }
        public string Name { get; }
    }
}