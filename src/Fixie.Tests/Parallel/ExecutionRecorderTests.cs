namespace Fixie.Tests.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;
    using Fixie.Reports;

    public class ExecutionRecorderTests
    {
        public async Task ShouldOnlyEmitOneEventAtATime()
        {
            const int testCount = 100;

            var console = TextWriter.Null;
            var reporter = new BlockingReporter();
            var bus = new Bus(console, new []{ reporter });
            var recordingConsole = new RecordingWriter(console);
            var recorder = new ExecutionRecorder(recordingConsole, bus);

            var test = new Test(recorder, this.GetType().GetMethod(nameof(BlankMethod), BindingFlags.Instance | BindingFlags.NonPublic)!);
            var messageTasks = new List<Task>();

            for (int i = 0; i < testCount; ++i)
            {
                messageTasks.Add((i % 6) switch
                {
                    0 => recorder.StartExecution(),
                    1 => recorder.Start(test),
                    2 => recorder.Pass(test, $"{i}"),
                    3 => recorder.Fail(test, $"{i}", new Exception("foo")),
                    4 => recorder.Skip(test, $"{i}", "foo"),
                    5 => recorder.CompleteExecution(),
                    _ => throw new ShouldBeUnreachableException()
                });
            }

            await Task.Delay(10);

            for (int i = 0; i < testCount; ++i)
            {
                reporter.Counter.ShouldBe(i);

                reporter.Next();
                var messageTask = await Task.WhenAny(messageTasks);
                await messageTask;

                reporter.Counter.ShouldBe(i + 1);
                messageTasks.Remove(messageTask);
            }

            messageTasks.Count.ShouldBe(0);

        }

        private void BlankMethod()
        {

        }

        class BlockingReporter : IReport,
            IHandler<ExecutionStarted>,
            IHandler<ExecutionCompleted>,
            IHandler<TestStarted>,
            IHandler<TestFailed>,
            IHandler<TestSkipped>,
            IHandler<TestPassed>
        {
            public int Counter => _counter;

            public void Next()
            {
                _semaphore.Release();
            }

            public Task Handle(ExecutionStarted message) => HandleMessage(message);
            public Task Handle(TestStarted message) => HandleMessage(message);
            public Task Handle(TestFailed message) => HandleMessage(message);
            public Task Handle(TestSkipped message) => HandleMessage(message);
            public Task Handle(TestPassed message) => HandleMessage(message);
            public Task Handle(ExecutionCompleted message) => HandleMessage(message);

            private async Task HandleMessage(IMessage message)
            {
                if (_busy)
                {
                    throw new FailureException();
                }

                _busy = true;
                try
                {
                    await _semaphore.WaitAsync();
                }
                finally
                {
                    ++_counter;
                    _busy = false;
                }
            }

            private bool _busy = false;
            private int _counter = 0;
            private SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);
        }
    }
}
