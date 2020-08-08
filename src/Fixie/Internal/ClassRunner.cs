namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    class ClassRunner
    {
        static readonly object[] EmptyParameters = {};
        static readonly object[][] InvokeOnceWithZeroParameters = { EmptyParameters };

        readonly Bus bus;
        readonly Execution execution;
        readonly ParameterDiscoverer parameterDiscoverer;
        
        ExecutionSummary classSummary;
        readonly Stopwatch classStopwatch;
        readonly Stopwatch caseStopwatch;

        public ClassRunner(Bus bus, Discovery discovery, Execution execution)
        {
            this.bus = bus;
            this.execution = execution;
            parameterDiscoverer = new ParameterDiscoverer(discovery);
            
            classSummary = new ExecutionSummary();
            classStopwatch = new Stopwatch();
            caseStopwatch = new Stopwatch();
        }

        public ExecutionSummary Run(Type testClass, bool isOnlyTestClass, IReadOnlyList<MethodInfo> testMethods)
        {
            Start(testClass);

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
                    Fail(testMethod, exception);
            }

            if (!runContext.Invoked && !classLifecycleFailed)
            {
                //No cases ran, and we didn't already emit a general
                //failure for each test method, so emit a general skip for
                //each test method.
                foreach (var testMethod in testMethods)
                    Skip(testMethod);
            }

            Complete(testClass);

            return classSummary;
        }

        void Run(MethodInfo testMethod, Action<Case> caseLifecycle)
        {
            Start(testMethod);

            try
            {
                bool invoked = false;

                var lazyInvocations = testMethod.GetParameters().Length == 0
                    ? InvokeOnceWithZeroParameters
                    : parameterDiscoverer.GetParameters(testMethod);

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
                Fail(testMethod, exception);
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
                    Fail(@case, output);
                else if (caseLifecycleFailure == null)
                    Pass(@case, output);
            }

            if (caseLifecycleFailure != null)
                Fail(new Case(@case, caseLifecycleFailure));
            else if (!caseHasNormalResult)
                Skip(@case, output);
        }

        void Start(Type testClass)
        {
            classSummary = new ExecutionSummary();
            bus.Publish(new ClassStarted(testClass));
            classStopwatch.Restart();
            caseStopwatch.Restart();
        }

        void Start(MethodInfo testMethod)
        {
            var test = new Test(testMethod);
            bus.Publish(new TestStarted(test));
        }

        void Skip(Case @case, string output = "")
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CaseSkipped(@case, duration, output);
            classSummary.Add(message);
            bus.Publish(message);
        }

        void Skip(MethodInfo testMethod)
        {
            var @case = new Case(testMethod, EmptyParameters);
            Skip(@case);
        }

        void Pass(Case @case, string output)
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CasePassed(@case, duration, output);
            classSummary.Add(message);
            bus.Publish(message);
        }

        void Fail(Case @case, string output = "")
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CaseFailed(@case, duration, output);
            classSummary.Add(message);
            bus.Publish(message);
        }

        void Fail(MethodInfo testMethod, Exception exception)
        {
            var @case = new Case(testMethod, EmptyParameters);
            @case.Fail(exception);
            Fail(@case);
        }

        void Complete(Type testClass)
        {
            var duration = classStopwatch.Elapsed;
            classStopwatch.Stop();
            bus.Publish(new ClassCompleted(testClass, classSummary, duration));
        }
    }
}