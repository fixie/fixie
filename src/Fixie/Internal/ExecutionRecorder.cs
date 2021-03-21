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

        public async Task StartAsync(Test test)
        {
            await bus.PublishAsync(new TestStarted(test));
        }

        public async Task SkipAsync(Case @case, string output, string? reason)
        {
            var duration = caseStopwatch.Elapsed;

            var message = new TestSkipped(@case, duration, output, reason);
            assemblySummary.Add(message);
            await bus.PublishAsync(message);

            caseStopwatch.Restart();
        }

        public async Task<TestPassed> PassAsync(Case @case, string output)
        {
            var duration = caseStopwatch.Elapsed;

            var message = new TestPassed(@case, duration, output);
            assemblySummary.Add(message);
            await bus.PublishAsync(message);

            caseStopwatch.Restart();

            return message;
        }

        public async Task<TestFailed> FailAsync(Case @case, string output, Exception reason)
        {
            var duration = caseStopwatch.Elapsed;

            var message = new TestFailed(@case, duration, output, reason);
            assemblySummary.Add(message);
            await bus.PublishAsync(message);

            caseStopwatch.Restart();

            return message;
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