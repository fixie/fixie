namespace Fixie.Internal
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Reports;

    class ExecutionRecorder
    {
        readonly RecordingWriter recordingConsole;
        readonly Bus bus;

        readonly ExecutionSummary assemblySummary;
        readonly Stopwatch assemblyStopwatch;
        readonly Stopwatch caseStopwatch;

        public ExecutionRecorder(RecordingWriter recordingConsole, Bus bus)
        {
            this.recordingConsole = recordingConsole;
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
            recordingConsole.StartRecording();
        }

        public async Task StartAsync(Test test)
        {
            await bus.PublishAsync(new TestStarted(test));
        }

        public async Task SkipAsync(Case @case, string? reason)
        {
            var duration = caseStopwatch.Elapsed;
            recordingConsole.StopRecording(out var output);

            var message = new TestSkipped(@case.Test.Name, @case.Name, duration, output, reason);
            assemblySummary.Add(message);
            await bus.PublishAsync(message);

            caseStopwatch.Restart();
            recordingConsole.StartRecording();
        }

        public async Task PassAsync(Case @case)
        {
            var duration = caseStopwatch.Elapsed;
            recordingConsole.StopRecording(out var output);

            var message = new TestPassed(@case.Test.Name, @case.Name, duration, output);
            assemblySummary.Add(message);
            await bus.PublishAsync(message);

            caseStopwatch.Restart();
            recordingConsole.StartRecording();
        }

        public async Task FailAsync(Case @case, Exception reason)
        {
            var duration = caseStopwatch.Elapsed;
            recordingConsole.StopRecording(out var output);

            var message = new TestFailed(@case.Test.Name, @case.Name, duration, output, reason);
            assemblySummary.Add(message);
            await bus.PublishAsync(message);

            caseStopwatch.Restart();
            recordingConsole.StartRecording();
        }

        public async Task<ExecutionSummary> CompleteAsync(TestAssembly testAssembly)
        {
            var duration = assemblyStopwatch.Elapsed;
            recordingConsole.StopRecording();
            caseStopwatch.Stop();
            assemblyStopwatch.Stop();

            await bus.PublishAsync(new AssemblyCompleted(testAssembly.Assembly, assemblySummary, duration));

            return assemblySummary;
        }
    }
}