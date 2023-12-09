using System.Diagnostics.CodeAnalysis;

namespace Fixie.TestAdapter;

class DebuggerAttachmentFailure
{
    public string Message { get; }
    public Exception ThirdPartyTestHostException { get; }

    public DebuggerAttachmentFailure(Exception thirdPartyTestHostException)
    {
        ThirdPartyTestHostException = thirdPartyTestHostException;
        Message = UserGuidanceMessage(thirdPartyTestHostException);
    }

    static string UserGuidanceMessage(Exception thirdPartyTestHostException)
    {
        var host = TryGetTestHost(out var testHost)
            ? $"{Environment.NewLine}{testHost}{Environment.NewLine}"
            : "";

        return $@"Fixie attempted to run your test assembly
under the active debugger session, but the
third-party test host {host}failed to honor the request to attach to the
test assembly process. The run continued
without the debugger so that this message
could be reported.


In order to debug your test, bypass your
test runner's unimplemented ""Debug""
option using one of the following two
approaches.

If your operating system supports
Debugger.Launch():

    1. Add the following line at the start
       of your test:

       System.Diagnostics.Debugger.Launch();

    2. Use your test runner's ""Run"" option
       to start the test instead of using
       its unimplemented ""Debug"" option.

    3. Make a selection in the resulting
       debugger session dialog.


If your operating system does not support
Debugger.Launch():

    1. Note that Fixie test projects are in
       fact normal console applications that
       run their own tests.

    2. Instead of using your test runner,
       run your test project under your
       development environment's debugger
       as a normal console application.


When filing a bug report to your specific
third-party test runner's organization,
please include the following exception
thrown by their implementation of the VSTest
API. Test runners are meant to implement
IFrameworkHandle.LaunchProcessWithDebuggerAttached
since they have control over the active
debugger session.


{thirdPartyTestHostException.Message}";
    }

    static bool TryGetTestHost([NotNullWhen(true)] out string? testHost)
    {
        try
        {
            testHost = Environment.GetCommandLineArgs().First();
        }
        catch
        {
            testHost = null;
        }

        return testHost != null;
    }
}