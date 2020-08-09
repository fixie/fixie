namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class ClassRunner
    {
        static readonly object[] EmptyParameters = {};
        static readonly object[][] InvokeOnceWithZeroParameters = { EmptyParameters };

        readonly ExecutionRecorder recorder;
        readonly Execution execution;
        readonly ParameterGenerator parameterGenerator;

        public ClassRunner(ExecutionRecorder recorder, Discovery discovery, Execution execution)
        {
            this.recorder = recorder;
            this.execution = execution;
            parameterGenerator = new ParameterGenerator(discovery);
        }

        public void Run(Type testClass, IReadOnlyList<MethodInfo> testMethods, MethodInfo? targetMethod)
        {
            recorder.Start(testClass);

            Action<Action<Case>> runCases = caseLifecycle =>
            {
                foreach (var testMethod in testMethods)
                    Run(testMethod, caseLifecycle);
            };

            var runContext = new TestClass(testClass, runCases, targetMethod);
            
            Exception? classLifecycleFailure = null;

            try
            {
                execution.Execute(runContext);
            }
            catch (Exception exception)
            {
                classLifecycleFailure = exception;
            }

            if (classLifecycleFailure != null)
            {
                foreach (var testMethod in testMethods)
                    recorder.Fail(testMethod, classLifecycleFailure);
            }
            else if (!runContext.Invoked)
            {
                foreach (var testMethod in testMethods)
                    recorder.Skip(testMethod);
            }
            
            recorder.Complete(testClass);
        }

        void Run(MethodInfo testMethod, Action<Case> caseLifecycle)
        {
            recorder.Start(testMethod);

            try
            {
                bool invoked = false;

                var lazyInvocations = testMethod.GetParameters().Length == 0
                    ? InvokeOnceWithZeroParameters
                    : parameterGenerator.GetParameters(testMethod);

                foreach (var parameters in lazyInvocations)
                {
                    invoked = true;

                    var @case = new Case(testMethod, parameters);

                    Run(@case, caseLifecycle);
                }

                if (!invoked)
                    throw new Exception("This test has declared parameters, but no parameter values have been provided to it.");
            }
            catch (Exception exception)
            {
                recorder.Fail(testMethod, exception);
            }
        }

        void Run(Case @case, Action<Case> caseLifecycle)
        {
            Exception? caseLifecycleFailure = null;

            string output;
            using (var console = new RedirectedConsole())
            {
                try
                {
                    caseLifecycle(@case);
                }
                catch (Exception exception)
                {
                    caseLifecycleFailure = exception;
                }

                output = console.Output;
            }

            Console.Write(output);

            if (@case.State == CaseState.Failed || @case.State == CaseState.Passed)
            {
                if (@case.State == CaseState.Failed)
                    recorder.Fail(@case, output);
                else if (caseLifecycleFailure == null)
                    recorder.Pass(@case, output);
            }

            if (caseLifecycleFailure != null)
                recorder.Fail(new Case(@case, caseLifecycleFailure));
            else if (@case.State == CaseState.Skipped)
                recorder.Skip(@case, output);
        }
    }
}