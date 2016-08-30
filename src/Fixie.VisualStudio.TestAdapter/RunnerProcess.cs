namespace Fixie.VisualStudio.TestAdapter
{
    using System.Diagnostics;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

    public class RunnerProcess
    {
        readonly IMessageLogger log;
        readonly string[] arguments;

        public RunnerProcess(IMessageLogger log, params string[] arguments)
        {
            this.log = log;
            this.arguments = arguments;
        }

        public void Run()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = typeof(Runner.Program).Assembly.Location,
                Arguments = ArgumentEscaper.EscapeForProcessStartInfo(arguments),
                UseShellExecute = false
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                log.Info($"{startInfo.FileName} {startInfo.Arguments}");
                process.EnableRaisingEvents = true;
                process.Start();
                process.WaitForExit();
            }
        }
    }
}