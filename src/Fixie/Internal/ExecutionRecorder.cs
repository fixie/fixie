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

        public async Task StartExecution()
        {
            await bus.Publish(new ExecutionStarted());
            assemblyStopwatch.Restart();
            caseStopwatch.Restart();
            recordingConsole.StartRecording();
        }

        public async Task Start(Test test)
        {
            await bus.Publish(new TestStarted(test));
        }

        public async Task Skip(Test test, string name, string reason)
        {
            var duration = caseStopwatch.Elapsed;
            recordingConsole.StopRecording(out var output);

            var message = new TestSkipped(test.Name, name, duration, output, reason);
            assemblySummary.Add(message);
            await bus.Publish(message);

            caseStopwatch.Restart();
            recordingConsole.StartRecording();
        }

        public async Task Pass(Test test, string name)
        {
            var duration = caseStopwatch.Elapsed;
            recordingConsole.StopRecording(out var output);

            var message = new TestPassed(test.Name, name, duration, output);
            assemblySummary.Add(message);
            await bus.Publish(message);

            caseStopwatch.Restart();
            recordingConsole.StartRecording();
        }

        public async Task Fail(Test test, string name, Exception reason)
        {
            var duration = caseStopwatch.Elapsed;
            recordingConsole.StopRecording(out var output);

            var message = new TestFailed(test.Name, name, duration, output, reason);
            assemblySummary.Add(message);
            await bus.Publish(message);

            caseStopwatch.Restart();
            recordingConsole.StartRecording();
        }

        public async Task<ExecutionSummary> CompleteExecution()
        {
            var duration = assemblyStopwatch.Elapsed;
            recordingConsole.StopRecording();
            caseStopwatch.Stop();
            assemblyStopwatch.Stop();

            await bus.Publish(new ExecutionCompleted(assemblySummary, duration));

            return assemblySummary;
        }
    }
}