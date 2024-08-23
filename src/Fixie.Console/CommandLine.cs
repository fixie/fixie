namespace Fixie.Console;

public class CommandLine
{
    public static T Parse<T>(string[] arguments) where T : class
        => new Parser<T>(arguments).Model;

    public static void Partition(string[] arguments, out string[] runnerArguments, out string[] customArguments)
    {
        var index = Array.IndexOf(arguments, "--");

        if (index >= 0)
        {
            runnerArguments = arguments[..index];
            customArguments = arguments[(index + 1)..];
        }
        else
        {
            runnerArguments = [..arguments];
            customArguments = [];
        }
    }
}