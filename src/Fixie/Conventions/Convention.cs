﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Conventions
{
    public class Convention
    {
        Func<MethodInfo, IEnumerable<object[]>> methodCallParameterBuilder;

        public Convention()
        {
            Classes = new ClassFilter().Where(type => !type.IsSubclassOf(typeof(Convention)));
            Methods = new MethodFilter().Where(m => !m.IsDispose());
            CaseBuilder = new DefaultCaseBuilder();
            CaseExecution = new CaseBehaviorBuilder();
            InstanceExecution = new InstanceBehaviorBuilder();
            ClassExecution = new TypeBehaviorBuilder().CreateInstancePerCase();

            methodCallParameterBuilder = method => new[] { (object[])null };
        }

        public ClassFilter Classes { get; private set; }
        public MethodFilter Methods { get; private set; }
        public ICaseBuilder CaseBuilder { get; protected set; }
        public CaseBehaviorBuilder CaseExecution { get; private set; }
        public InstanceBehaviorBuilder InstanceExecution { get; private set; }
        public TypeBehaviorBuilder ClassExecution { get; private set; }

        public void Parameters(Func<MethodInfo, IEnumerable<object[]>> getCaseParameters)
        {
            methodCallParameterBuilder = getCaseParameters;
        }

        public void Execute(Listener listener, params Type[] candidateTypes)
        {
            foreach (var testClass in Classes.Filter(candidateTypes))
            {
                var methods = Methods.Filter(testClass);

                var cases = methods.SelectMany(method =>
                    methodCallParameterBuilder(method).Select(parameters =>
                        CaseBuilder.Build(testClass, method, parameters))).ToArray();

                ClassExecution.Behavior.Execute(testClass, this, cases);

                foreach (var @case in cases)
                {
                    var exceptions = @case.Exceptions;

                    if (exceptions.Any())
                        listener.CaseFailed(@case, exceptions.ToArray());
                    else
                        listener.CasePassed(@case);
                }
            }
        }
    }
}