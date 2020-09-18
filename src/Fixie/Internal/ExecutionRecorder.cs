namespace Fixie.Internal
{
    using System;
    using System.Diagnostics;

    class ExecutionRecorder
    {
        static readonly object[] EmptyParameters = { };

        readonly Bus bus;

        readonly ExecutionSummary assemblySummary;
        readonly Stopwatch assemblyStopwatch;
        readonly Stopwatch caseStopwatch;

        public ExecutionRecorder(Bus bus)
        {
            this.bus = bus;

            assemblySummary = new ExecutionSummary();
            
            assemblyStopwatch = new Stopwatch();
            caseStopwatch = new Stopwatch();
        }

        public void Start(TestAssembly testAssembly)
        {
            bus.Publish(new AssemblyStarted(testAssembly.Assembly));
            assemblyStopwatch.Restart();
            caseStopwatch.Restart();
        }

        public void Start(Case @case)
        {
            bus.Publish(new CaseStarted(@case));
        }

        public void Skip(Case @case, string output = "")
        {
            var duration = caseStopwatch.Elapsed;

            var message = new CaseSkipped(@case, duration, output);
            assemblySummary.Add(message);
            bus.Publish(message);

            caseStopwatch.Restart();
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

            var message = new CasePassed(@case, duration, output);
            assemblySummary.Add(message);
            bus.Publish(message);

            caseStopwatch.Restart();
        }

        public void Fail(Case @case, string output = "")
        {
            var duration = caseStopwatch.Elapsed;

            var message = new CaseFailed(@case, duration, output);
            assemblySummary.Add(message);
            bus.Publish(message);

            caseStopwatch.Restart();
        }

        public void Fail(TestMethod testMethod, Exception reason)
        {
            var @case = new Case(testMethod.Method, EmptyParameters);
            @case.Fail(reason);
            Fail(@case);
        }

        public ExecutionSummary Complete(TestAssembly testAssembly)
        {
            var duration = assemblyStopwatch.Elapsed;

            bus.Publish(new AssemblyCompleted(testAssembly.Assembly, assemblySummary, duration));

            caseStopwatch.Stop();
            assemblyStopwatch.Stop();
            return assemblySummary;
        }
    }
}