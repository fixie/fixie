namespace Fixie.Runner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using Cli;
    using Execution;
    using Microsoft.Extensions.Testing.Abstractions;
    using Newtonsoft.Json;
    using Reports;
    using Message = Microsoft.Extensions.Testing.Abstractions.Message;

    class Program
    {
        const int FatalError = -1;

        [STAThread]
        static int Main(string[] arguments)
        {
            var runnerArguments = new List<string>();
            var conventionArguments = new List<string>();

            bool separatorFound = false;
            foreach (var arg in arguments)
            {
                if (arg == "--")
                {
                    separatorFound = true;
                    continue;
                }

                if (separatorFound)
                    conventionArguments.Add(arg);
                else
                    runnerArguments.Add(arg);
            }

            try
            {
                Options options;
                try
                {
                    string[] unusedArguments;
                    options = CommandLine.Parse<Options>(runnerArguments, out unusedArguments);

                    using (Foreground.Yellow)
                        foreach (var unusedArgument in unusedArguments)
                            Console.WriteLine($"The argument '{unusedArgument}' was unexpected and will be ignored.");

                    options.Validate();
                }
                catch (CommandLineException exception)
                {
                    using (Foreground.Red)
                        Console.WriteLine(exception.Message);

                    Console.WriteLine();
                    Console.WriteLine(Usage());
                    return FatalError;
                }

                if (options.DesignTime)
                {
                    RunAssemblyForIde(options, conventionArguments);
                    return 0;
                }

                return RunAssemblyForConsole(options, conventionArguments);
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception}");
                return FatalError;
            }
        }

        static void RunAssemblyForIde(Options options, IReadOnlyList<string> conventionArguments)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Loopback, options.Port.Value);
                socket.Connect(ipEndPoint);

                using (var networkStream = new NetworkStream(socket))
                using (var writer = new BinaryWriter(networkStream))
                using (var reader = new BinaryReader(networkStream))
                using (var sink = new DesignTimeSink(writer))
                {
                    if (options.List)
                    {
                        using (var environment = new ExecutionEnvironment(options.AssemblyPath))
                        {
                            environment.Subscribe<DesignTimeDiscoveryListener>(sink);
                            environment.DiscoverMethodGroups(conventionArguments.ToArray());
                        }

                        sink.SendTestCompleted();
                    }
                    else if (options.WaitCommand)
                    {
                        sink.SendWaitingCommand();

                        var rawMessage = reader.ReadString();
                        var message = JsonConvert.DeserializeObject<Message>(rawMessage);
                        var testsToRun = message.Payload.ToObject<RunTestsMessage>().Tests;

                        if (testsToRun.Any())
                        {
                            using (var environment = new ExecutionEnvironment(options.AssemblyPath))
                            {
                                environment.Subscribe<DesignTimeExecutionListener>(sink);
                                var methodGroups = testsToRun.Select(x => new MethodGroup(x)).ToArray();
                                environment.RunMethods(methodGroups, conventionArguments.ToArray());
                            }
                        }
                        else
                        {
                            using (var environment = new ExecutionEnvironment(options.AssemblyPath))
                            {
                                environment.Subscribe<DesignTimeExecutionListener>(sink);
                                environment.RunAssembly(conventionArguments.ToArray());
                            }
                        }

                        sink.SendTestCompleted();
                    }
                }
            }
        }

        static int RunAssemblyForConsole(Options options, IReadOnlyList<string> conventionArguments)
        {
            using (var environment = new ExecutionEnvironment(options.AssemblyPath))
            {
                if (ShouldUseTeamCityListener(options))
                    environment.Subscribe<TeamCityListener>();
                else
                    environment.Subscribe<ConsoleListener>();

                if (ShouldUseAppVeyorListener())
                    environment.Subscribe<AppVeyorListener>();

                if (options.ReportFormat == ReportFormat.NUnit)
                    environment.Subscribe<ReportListener<NUnitXml>>();
                else if (options.ReportFormat == ReportFormat.xUnit)
                    environment.Subscribe<ReportListener<XUnitXml>>();

                return environment.RunAssembly(conventionArguments.ToArray());
            }
        }

        static bool ShouldUseTeamCityListener(Options options)
        {
            var teamCityExplicitlyEnabled = options.TeamCity == true;

            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            return teamCityExplicitlyEnabled || runningUnderTeamCity;
        }

        static bool ShouldUseAppVeyorListener()
        {
            return Environment.GetEnvironmentVariable("APPVEYOR") == "True";
        }

        static string Usage()
        {
            //TODO: As of .NET Core 1.0.0 VS 2015 Tooling Preview 2, it appears that you cannot have a -- separator yet
            //      between [dotnet arguments] and Fixie's own arguments. The implementation of https://github.com/dotnet/cli/blob/rel/1.0.0/src/dotnet/CommandLine/CommandLineApplication.cs
            //      on master, though, suggests that the omission has already been fixed. Once that fix is deployed, this usage
            //      will need to change to reflect the corrected behavior.

            return new StringBuilder()
                .AppendLine("Usage: dotnet test [dotnet arguments]")
                .AppendLine("       dotnet test [dotnet arguments] [--report-format <NUnit|xUnit>] [--team-city <on|off>] [--] [convention arguments]...")
                .AppendLine()
                .AppendLine()
                .AppendLine("    dotnet arguments")
                .AppendLine("        Arguments to be used by 'dotnet test'. See 'dotnet test --help'.")
                .AppendLine()
                .AppendLine("    --report-format <NUnit|xUnit>")
                .AppendLine("        Write test results to a file, using NUnit or xUnit XML format.")
                .AppendLine()
                .AppendLine("    --team-city <on|off>")
                .AppendLine("        When this option is *not* specified, the need for TeamCity-")
                .AppendLine("        formatted console output is automatically detected. Use this")
                .AppendLine("        option to force TeamCity-formatted output on or off.")
                .AppendLine()
                .AppendLine("    --")
                .AppendLine("        When present, all of the following arguments will be passed along")
                .AppendLine("        for use from within a convention.")
                .AppendLine()
                .AppendLine("    convention arguments")
                .AppendLine("        Arbitrary arguments made available to conventions at runtime.")
                .ToString();
        }
    }
}