namespace Fixie.Internal
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    class ClassRunner
    {
        static readonly object[] EmptyParameters = {};
        static readonly object[][] InvokeOnceWithZeroParameters = { EmptyParameters };

        readonly Bus bus;
        readonly Execution execution;
        readonly MethodDiscoverer methodDiscoverer;
        readonly ParameterDiscoverer parameterDiscoverer;
        readonly Stopwatch caseStopwatch;

        public ClassRunner(Bus bus, Discovery discovery, Execution execution)
        {
            this.bus = bus;
            this.execution = execution;
            methodDiscoverer = new MethodDiscoverer(discovery);
            parameterDiscoverer = new ParameterDiscoverer(discovery);
            caseStopwatch = new Stopwatch();
        }

        public ExecutionSummary Run(Type testClass, bool isOnlyTestClass)
        {
            var testMethods = methodDiscoverer.TestMethods(testClass);

            var summary = new ExecutionSummary();

            if (!testMethods.Any())
                return summary;

            Start(testClass);

            var classStopwatch = Stopwatch.StartNew();
            caseStopwatch.Restart();

            bool classLifecycleFailed = false;

            Action<Action<Case>> runCases = caseLifecycle =>
            {
                foreach (var testMethod in testMethods)
                    Run(testMethod, caseLifecycle, summary);
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
                    Fail(testMethod, exception, summary);
            }

            if (!runContext.Invoked && !classLifecycleFailed)
            {
                //No cases ran, and we didn't already emit a general
                //failure for each test method, so emit a general skip for
                //each test method.
                foreach (var testMethod in testMethods)
                    Skip(testMethod, summary);
            }

            classStopwatch.Stop();
            Complete(testClass, summary, classStopwatch.Elapsed);

            return summary;
        }

        void Run(MethodInfo testMethod, Action<Case> caseLifecycle, ExecutionSummary summary)
        {
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

                    Run(@case, caseLifecycle, summary);
                }

                if (!invoked)
                    throw new Exception("This test has declared parameters, but no parameter values have been provided to it.");
            }
            catch (Exception exception)
            {
                Fail(testMethod, exception, summary);
            }
        }

        void Run(Case @case, Action<Case> caseLifecycle, ExecutionSummary summary)
        {
            Start(@case);

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
                    Fail(@case, summary, output);
                else if (caseLifecycleFailure == null)
                    Pass(@case, summary, output);
            }

            if (caseLifecycleFailure != null)
                Fail(new Case(@case, caseLifecycleFailure), summary);
            else if (!caseHasNormalResult)
                Skip(@case, summary, output);
        }

        void Start(Type testClass)
        {
            bus.Publish(new ClassStarted(testClass));
        }

        void Start(Case @case)
        {
            bus.Publish(new CaseStarted(@case));
        }

        void Skip(Case @case, ExecutionSummary summary, string output = "")
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CaseSkipped(@case, duration, output);
            summary.Add(message);
            bus.Publish(message);
        }

        void Skip(MethodInfo testMethod, ExecutionSummary summary)
        {
            var @case = new Case(testMethod, EmptyParameters);
            Skip(@case, summary);
        }

        void Pass(Case @case, ExecutionSummary summary, string output)
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CasePassed(@case, duration, output);
            summary.Add(message);
            bus.Publish(message);
        }

        void Fail(Case @case, ExecutionSummary summary, string output = "")
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CaseFailed(@case, duration, output);
            summary.Add(message);
            bus.Publish(message);
        }

        void Fail(MethodInfo testMethod, Exception exception, ExecutionSummary summary)
        {
            var @case = new Case(testMethod, EmptyParameters);
            @case.Fail(exception);
            Fail(@case, summary);
        }

        void Complete(Type testClass, ExecutionSummary summary, TimeSpan duration)
        {
            bus.Publish(new ClassCompleted(testClass, summary, duration));
        }
    }
}