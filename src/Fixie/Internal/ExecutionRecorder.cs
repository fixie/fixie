namespace Fixie.Internal
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Reports;

    class ExecutionRecorder
    {
        readonly RecordingWriter recordingConsole;
        readonly Bus bus;

        readonly ExecutionSummary assemblySummary;
        readonly Stopwatch assemblyStopwatch;
        readonly AsyncLock eventLock;

        public ExecutionRecorder(RecordingWriter recordingConsole, Bus bus)
        {
            this.recordingConsole = recordingConsole;
            this.bus = bus;

            assemblySummary = new ExecutionSummary();

            assemblyStopwatch = new Stopwatch();
            eventLock = new AsyncLock();
        }

        public async Task StartExecution()
        {
            using (await eventLock.LockAsync())
            {
                await bus.Publish(new ExecutionStarted());

                assemblyStopwatch.Restart();
                recordingConsole.StartRecording();
            }
        }

        public async Task<ExecutingTest> Start(Test test)
        {
            using (await eventLock.LockAsync())
            {
                var executingTest = new ExecutingTest(test);
                await bus.Publish(new TestStarted(test));
                return executingTest;
            }
        }

        public Task Skip(ExecutingTest executingTest, string name, string reason) => Skip(executingTest.Test.Name, name, executingTest.Elapsed, reason);
        public Task Skip(Test test, string name, string reason) => Skip(test.Name, name, TimeSpan.Zero, reason);
        async Task Skip(string test, string testCase, TimeSpan duration, string reason)
        {
            using (await eventLock.LockAsync())
            {
                recordingConsole.StopRecording(out var output);

                var message = new TestSkipped(test, testCase, duration, output, reason);
                assemblySummary.Add(message);
                await bus.Publish(message);

                recordingConsole.StartRecording();
            }
        }

        public Task Pass(ExecutingTest executingTest, string name) => Pass(executingTest.Test.Name, name, executingTest.Elapsed);
        public Task Pass(Test test, string name) => Pass(test.Name, name, TimeSpan.Zero);
        async Task Pass(string test, string testCase, TimeSpan duration)
        {
            using (await eventLock.LockAsync())
            {
                recordingConsole.StopRecording(out var output);

                var message = new TestPassed(test, testCase, duration, output);
                assemblySummary.Add(message);
                await bus.Publish(message);

                recordingConsole.StartRecording();
            }
        }

        public Task Fail(ExecutingTest executingTest, string name, Exception reason) => Fail(executingTest.Test.Name, name, executingTest.Elapsed, reason);
        public Task Fail(Test test, string name, Exception reason) => Fail(test.Name, name, TimeSpan.Zero, reason);
        async Task Fail(string test, string testCase, TimeSpan duration, Exception reason)
        {
            using (await eventLock.LockAsync())
            {
                recordingConsole.StopRecording(out var output);

                var message = new TestFailed(test, testCase, duration, output, reason);
                assemblySummary.Add(message);
                await bus.Publish(message);

                recordingConsole.StartRecording();
            }
        }

        public async Task<ExecutionSummary> CompleteExecution()
        {
            using (await eventLock.LockAsync())
            {
                var duration = assemblyStopwatch.Elapsed;
                recordingConsole.StopRecording();
                assemblyStopwatch.Stop();

                await bus.Publish(new ExecutionCompleted(assemblySummary, duration));

                return assemblySummary;
            }
        }
    }
}