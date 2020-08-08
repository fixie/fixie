namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        public void Run(Type testClass, bool isOnlyTestClass, IReadOnlyList<MethodInfo> testMethods)
        {
            recorder.Start(testClass);

            bool classLifecycleFailed = false;

            Action<Action<Case>> runCases = caseLifecycle =>
            {
                foreach (var testMethod in testMethods)
                    Run(testMethod, caseLifecycle);
            };

            var runContext = isOnlyTestClass && testMethods.Count == 1
                ? new TestClass(testClass, runCases, testMethods.Single())
                : new TestClass(testClass, runCases);

            try
            {
                execution.Execute(runContext);
            }
            catch (Exception exception)
            {
                classLifecycleFailed = true;
                foreach (var testMethod in testMethods)
                    recorder.Fail(testMethod, exception);
            }

            if (!runContext.Invoked && !classLifecycleFailed)
            {
                //No cases ran, and we didn't already emit a general
                //failure for each test method, so emit a general skip for
                //each test method.
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

            var caseHasNormalResult = @case.State == CaseState.Failed || @case.State == CaseState.Passed;

            if (caseHasNormalResult)
            {
                if (@case.State == CaseState.Failed)
                    recorder.Fail(@case, output);
                else if (caseLifecycleFailure == null)
                    recorder.Pass(@case, output);
            }

            if (caseLifecycleFailure != null)
                recorder.Fail(new Case(@case, caseLifecycleFailure));
            else if (!caseHasNormalResult)
                recorder.Skip(@case, output);
        }
    }
}