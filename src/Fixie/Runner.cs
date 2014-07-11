using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;
using Fixie.Discovery;
using Fixie.Results;

namespace Fixie
{
    public class Runner
    {
        readonly Listener listener;
        readonly ILookup<string, string> options;

        public Runner(Listener listener)
            : this(listener, Enumerable.Empty<string>().ToLookup(x => x, x => x)) { }

        public Runner(Listener listener, ILookup<string, string> options)
        {
            this.listener = listener;
            this.options = options;
        }

        public AssemblyResult RunAssembly(Assembly assembly)
        {
            var runContext = new RunContext(assembly, options);

            return RunTypes(runContext, assembly.GetTypes());
        }

        public AssemblyResult RunNamespace(Assembly assembly, string ns)
        {
            var runContext = new RunContext(assembly, options);

            return RunTypes(runContext, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public AssemblyResult RunType(Assembly assembly, Type type)
        {
            var runContext = new RunContext(assembly, options, type);

            return RunTypes(runContext, type);
        }

        public AssemblyResult RunTypes(Assembly assembly, Convention convention, params Type[] types)
        {
            var runContext = new RunContext(assembly, options);

            return RunTypes(runContext, convention, types);
        }

        public AssemblyResult RunMethod(Assembly assembly, MethodInfo method)
        {
            var runContext = new RunContext(assembly, options, method);

            var conventions = GetConventions(runContext);

            foreach (var convention in conventions)
                convention.Methods.Where(m => m == method);

            var type = method.DeclaringType;

            return Run(runContext, conventions, type);
        }

        private AssemblyResult RunTypes(RunContext runContext, params Type[] types)
        {
            return Run(runContext, GetConventions(runContext), types);
        }

        private AssemblyResult RunTypes(RunContext runContext, Convention convention, params Type[] types)
        {
            return Run(runContext, new[] { convention }, types);
        }

        private static Convention[] GetConventions(RunContext runContext)
        {
            return new ConventionDiscoverer(runContext).GetConventions();
        }

        AssemblyResult Run(RunContext runContext, IEnumerable<Convention> conventions, params Type[] candidateTypes)
        {
            var assemblyResult = new AssemblyResult(runContext.Assembly.Location);
            
            listener.AssemblyStarted(runContext.Assembly);

            foreach (var convention in conventions)
            {
                var conventionResult = RunConvention(convention, candidateTypes);

                assemblyResult.Add(conventionResult);
            }

            listener.AssemblyCompleted(runContext.Assembly, assemblyResult);

            return assemblyResult;
        }

        ConventionResult RunConvention(Convention convention, params Type[] candidateTypes)
        {
            var config = convention.Config;
            var caseDiscoverer = new CaseDiscoverer(config);
            var executionModel = new ExecutionModel(config);
            var conventionResult = new ConventionResult(convention.GetType().FullName);

            foreach (var testClass in caseDiscoverer.TestClasses(candidateTypes))
            {
                var classResult = new ClassResult(testClass.FullName);

                var cases = caseDiscoverer.TestCases(testClass);
                var casesBySkipState = cases.ToLookup(executionModel.SkipCase);
                var casesToSkip = casesBySkipState[true];
                var casesToExecute = casesBySkipState[false].ToArray();
                foreach (var @case in casesToSkip)
                {
                    var skipResult = new SkipResult(@case, executionModel.GetSkipReason(@case));
                    listener.CaseSkipped(skipResult);
                    classResult.Add(CaseResult.Skipped(skipResult.Case.Name, skipResult.Reason));
                }

                if (casesToExecute.Any())
                {
                    executionModel.OrderCases(casesToExecute);

                    var caseExecutions = executionModel.Execute(testClass, casesToExecute);

                    foreach (var caseExecution in caseExecutions)
                    {
                        if (caseExecution.Exceptions.Any())
                        {
                            var failResult = new FailResult(caseExecution, executionModel.AssertionLibraryFilter);
                            listener.CaseFailed(failResult);
                            classResult.Add(CaseResult.Failed(failResult.Case.Name, failResult.Duration, failResult.ExceptionSummary));
                        }
                        else
                        {
                            var passResult = new PassResult(caseExecution);
                            listener.CasePassed(passResult);
                            classResult.Add(CaseResult.Passed(passResult.Case.Name, passResult.Duration));
                        }
                    }
                }

                conventionResult.Add(classResult);
            }

            return conventionResult;
        }
    }
}