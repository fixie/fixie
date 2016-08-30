namespace Fixie.VisualStudio.TestAdapter
{
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    public class RunnerProcess
    {
        readonly IMessageLogger log;
        readonly string[] arguments;
        readonly ManualResetEvent terminateWaitHandle;

        public RunnerProcess(IMessageLogger log, params string[] arguments)
        {
            this.log = log;
            this.arguments = arguments;
            terminateWaitHandle = new ManualResetEvent(false);
        }

        public void WaitForExit()
        {
            log.Info("Waiting for background process to exit.");
            terminateWaitHandle.WaitOne();
            log.Info("Background process exited.");
        }

        public void Start()
        {
            var fileName = typeof(Runner.Program).Assembly.Location;
            var escapedArguments = ArgumentEscaper.EscapeForProcessStartInfo(arguments);

            log.Info($"{fileName} {escapedArguments}");

            terminateWaitHandle.Reset();

            new Thread(() =>
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = escapedArguments,
                    UseShellExecute = false
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.EnableRaisingEvents = true;
                    process.Start();
                    process.WaitForExit();
                }

                terminateWaitHandle.Set();

            }) { IsBackground = true }.Start();
        }
    }
}