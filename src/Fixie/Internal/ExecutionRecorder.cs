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

        public async Task SkipAsync(Test test, string name, string reason)
        {
            var duration = caseStopwatch.Elapsed;
            recordingConsole.StopRecording(out var output);

            var message = new TestSkipped(test.Name, name, duration, output, reason);
            assemblySummary.Add(message);
            await bus.PublishAsync(message);

            caseStopwatch.Restart();
            recordingConsole.StartRecording();
        }

        public async Task PassAsync(Test test, string name)
        {
            var duration = caseStopwatch.Elapsed;
            recordingConsole.StopRecording(out var output);

            var message = new TestPassed(test.Name, name, duration, output);
            assemblySummary.Add(message);
            await bus.PublishAsync(message);

            caseStopwatch.Restart();
            recordingConsole.StartRecording();
        }

        public async Task FailAsync(Test test, string name, Exception reason)
        {
            var duration = caseStopwatch.Elapsed;
            recordingConsole.StopRecording(out var output);

            var message = new TestFailed(test.Name, name, duration, output, reason);
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