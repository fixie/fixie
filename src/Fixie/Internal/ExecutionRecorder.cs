namespace Fixie.Internal
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    class ExecutionRecorder
    {
        static readonly object[] EmptyParameters = { };

        readonly Bus bus;

        readonly ExecutionSummary assemblySummary;
        ExecutionSummary classSummary;

        readonly Stopwatch assemblyStopwatch;
        readonly Stopwatch classStopwatch;
        readonly Stopwatch caseStopwatch;

        public ExecutionRecorder(Bus bus)
        {
            this.bus = bus;

            assemblySummary = new ExecutionSummary();
            classSummary = new ExecutionSummary();
            
            assemblyStopwatch = new Stopwatch();
            classStopwatch = new Stopwatch();
            caseStopwatch = new Stopwatch();
        }

        public void Start(Assembly testAssembly)
        {
            bus.Publish(new AssemblyStarted(testAssembly));
            assemblyStopwatch.Restart();
        }

        public void Start(TestClass testClass)
        {
            classSummary = new ExecutionSummary();
            bus.Publish(new ClassStarted(testClass.Type));
            classStopwatch.Restart();
            caseStopwatch.Restart();
        }

        public void Start(Case @case)
        {
            bus.Publish(new CaseStarted(@case));
        }

        public void Skip(Case @case, string output = "")
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CaseSkipped(@case, duration, output);
            classSummary.Add(message);
            bus.Publish(message);
        }

        public void Skip(TestMethod testMethod, string? reason = null)
        {
            var @case = new Case(testMethod.Method, EmptyParameters);
            @case.Skip(reason);
            Skip(@case);
        }

        public void Pass(Case @case, string output)
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CasePassed(@case, duration, output);
            classSummary.Add(message);
            bus.Publish(message);
        }

        public void Fail(Case @case, string output = "")
        {
            var duration = caseStopwatch.Elapsed;
            caseStopwatch.Restart();

            var message = new CaseFailed(@case, duration, output);
            classSummary.Add(message);
            bus.Publish(message);
        }

        public void Fail(TestMethod testMethod, Exception reason)
        {
            var @case = new Case(testMethod.Method, EmptyParameters);
            @case.Fail(reason);
            Fail(@case);
        }

        public void Complete(TestClass testClass)
        {
            var duration = classStopwatch.Elapsed;
            classStopwatch.Stop();
            bus.Publish(new ClassCompleted(testClass.Type, classSummary, duration));
            assemblySummary.Add(classSummary);
        }

        public ExecutionSummary Complete(Assembly testAssembly)
        {
            assemblyStopwatch.Stop();
            bus.Publish(new AssemblyCompleted(testAssembly, assemblySummary, assemblyStopwatch.Elapsed));
            return assemblySummary;
        }
    }
}