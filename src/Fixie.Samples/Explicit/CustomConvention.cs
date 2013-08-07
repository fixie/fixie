﻿using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Samples.Explicit
{
    public class CustomConvention : Convention
    {
        public CustomConvention(RunContext runContext)
        {
            Classes
                .Where(type => type.IsInNamespace(GetType().Namespace))
                .NameEndsWith("Tests");

            Cases = new MethodFilter()
                .Where(method => method.Void())
                .ZeroParameters()
                .Where(method =>
                {
                    var isMarkedExplicit = method.Has<ExplicitAttribute>();

                    return !isMarkedExplicit || runContext.TargetMember == method;
                });

            ClassExecution
                .CreateInstancePerTestClass();
        }
    }
}