using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Discovery;
using Fixie.Execution;

namespace Fixie
{
    public class Case : BehaviorContext
    {
        public Case(MethodInfo caseMethod, params object[] parameters)
        {
            Parameters = parameters != null && parameters.Length == 0 ? null : parameters;
            Class = caseMethod.ReflectedType;

            Method = caseMethod.IsGenericMethodDefinition
                         ? caseMethod.MakeGenericMethod(GenericArgumentResolver.ResolveTypeArguments(caseMethod, parameters))
                         : caseMethod;

            Name = GetName();

            Execution = new CaseExecution();
        }

        string GetName()
        {
            var name = Class.FullName + "." + Method.Name;

            if (Method.IsGenericMethod)            
                name = string.Format("{0}<{1}>", name, string.Join(", ", Method.GetGenericArguments().Select(x => x.FullName)));

            if (Parameters != null && Parameters.Length > 0)
                name = string.Format("{0}({1})", name, string.Join(", ", Parameters.Select(x => x.ToDisplayString())));

            return name;
        }

        public string Name { get; private set; }
        public Type Class { get; private set; }
        public MethodInfo Method { get; private set; }
        public object[] Parameters { get; private set; }

        public CaseExecution Execution { get; private set; }
        public IReadOnlyList<Exception> Exceptions { get { return Execution.Exceptions; } }
        public void Fail(Exception reason)
        {
            Execution.Fail(reason);
        }
        public void ClearExceptions()
        {
            Execution.ClearExceptions();
        }

        public object Instance { get; internal set; }
        public TimeSpan Duration { get; internal set; }
        public string Output { get; internal set; }
        public object Result { get; internal set; }
    }
}
