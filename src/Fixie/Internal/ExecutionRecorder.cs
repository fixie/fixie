namespace Fixie.Internal
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Reports;

    class ExecutionRecorder
    {
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

        public async Task StartAsync(TestAssembly testAssembly)
        {
            await bus.PublishAsync(new AssemblyStarted(testAssembly.Assembly));
            assemblyStopwatch.Restart();
            caseStopwatch.Restart();
        }

        public async Task StartAsync(Case @case)
        {
            await bus.PublishAsync(new CaseStarted(@case));
        }

        public async Task SkipAsync(Case @case, string output = "")
        {
            var duration = caseStopwatch.Elapsed;

            var message = new CaseSkipped(@case, duration, output);
            assemblySummary.Add(message);
            await bus.PublishAsync(message);

            caseStopwatch.Restart();
        }

        public async Task SkipAsync(TestMethod testMethod, object?[] parameters, string? reason)
        {
            var @case = new Case(testMethod.Method, parameters);
            @case.Skip(reason);
            await SkipAsync(@case);
        }

        public async Task PassAsync(Case @case, string output)
        {
            var duration = caseStopwatch.Elapsed;

            var message = new CasePassed(@case, duration, output);
            assemblySummary.Add(message);
            await bus.PublishAsync(message);

            caseStopwatch.Restart();
        }

        public async Task FailAsync(Case @case, string output = "")
        {
            var duration = caseStopwatch.Elapsed;

            var message = new CaseFailed(@case, duration, output);
            assemblySummary.Add(message);
            await bus.PublishAsync(message);

            caseStopwatch.Restart();
        }

        public async Task FailAsync(TestMethod testMethod, object?[] parameters, Exception reason)
        {
            var @case = new Case(testMethod.Method, parameters);
            @case.Fail(reason);
            await FailAsync(@case);
        }

        public async Task<ExecutionSummary> CompleteAsync(TestAssembly testAssembly)
        {
            var duration = assemblyStopwatch.Elapsed;

            await bus.PublishAsync(new AssemblyCompleted(testAssembly.Assembly, assemblySummary, duration));

            caseStopwatch.Stop();
            assemblyStopwatch.Stop();
            return assemblySummary;
        }
    }
}