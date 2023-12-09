using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fixie.Reports;
using static System.Environment;
using static Fixie.Internal.Maybe;

namespace Fixie.Internal;

public class EntryPoint
{
    enum ExitCode
    {
        Success = 0,
        Failure = 1,
        FatalError = 2
    }

    public static async Task<int> Main(Assembly assembly, string[] customArguments)
    {
        var console = Console.Out;
        var rootPath = Directory.GetCurrentDirectory();
        var environment = new TestEnvironment(assembly, console, rootPath, customArguments);

        using var boundary = new ConsoleRedirectionBoundary();

        try
        {
            var pipeName = GetEnvironmentVariable("FIXIE_NAMED_PIPE");

            if (pipeName == null)
            {
                var reports = DefaultReports(environment).ToArray();

                var pattern = GetEnvironmentVariable("FIXIE_TESTS_PATTERN");

                return pattern == null
                    ? (int) await Run(environment, reports, async runner => await runner.Run())
                    : (int) await Run(environment, reports, async runner => await runner.Run(new TestPattern(pattern)));
            }

            using var pipeStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            using var pipe = new TestAdapterPipe(pipeStream);

            pipeStream.Connect();
            pipeStream.ReadMode = PipeTransmissionMode.Byte;
                
            var testAdapterReport = new TestAdapterReport(pipe);

            var exitCode = ExitCode.Success;

            try
            {
                var messageType = pipe.ReceiveMessageType();

                if (messageType == typeof(PipeMessage.DiscoverTests).FullName)
                {
                    var discoverTests = pipe.Receive<PipeMessage.DiscoverTests>();
                    await DiscoverMethods(environment, testAdapterReport);
                }
                else if (messageType == typeof(PipeMessage.ExecuteTests).FullName)
                {
                    var executeTests = pipe.Receive<PipeMessage.ExecuteTests>();

                    var reports = new IReport[] { testAdapterReport };

                    exitCode = executeTests.Filter.Length == 0
                        ? await Run(environment, reports, async runner => await runner.Run())
                        : await Run(environment, reports, async runner => await runner.Run(new HashSet<string>(executeTests.Filter)));
                }
                else
                {
                    var body = pipe.ReceiveMessageBody();
                    throw new Exception($"Test assembly received unexpected message of type {messageType}: {body}");
                }
            }
            catch (Exception exception)
            {
                pipe.Send(exception);
            }
            finally
            {
                pipe.Send<PipeMessage.EndOfPipe>();
            }

            return (int)exitCode;
        }
        catch (Exception exception)
        {
            using (Foreground.Red)
                console.WriteLine($"Fatal Error: {exception}");

            return (int)ExitCode.FatalError;
        }
    }

    static async Task DiscoverMethods(TestEnvironment environment, TestAdapterReport testAdapterReport)
    {
        var runner = new Runner(environment, testAdapterReport);
        await runner.Discover();
    }

    static async Task<ExitCode> Run(TestEnvironment environment, IReport[] reports, Func<Runner, Task<ExecutionSummary>> run)
    {
        var runner = new Runner(environment, reports);
            
        var summary = await run(runner);

        if (summary.Total == 0)
            return ExitCode.Failure;

        if (summary.Failed > 0)
            return ExitCode.Failure;

        return ExitCode.Success;
    }

    static IEnumerable<IReport> DefaultReports(TestEnvironment environment)
    {
        if (Try(AzureReport.Create, environment, out var azure))
            yield return azure;

        if (Try(AppVeyorReport.Create, environment, out var appVeyor))
            yield return appVeyor;

        if (Try(TeamCityReport.Create, environment, out var teamCity))
            yield return teamCity;

        yield return ConsoleReport.Create(environment);
    }
}