// Test stub for F#'s Async<'T> such that we can
// reflect on it in the same way as we can against
// the real implementation during F# test runs.

namespace Microsoft.FSharp.Control;

public class FSharpAsync<TResult>(Func<TResult> result)
{
    public FSharpAsync(TResult result) : this(() => result) { }

    public TResult Result => result();
}

public class FSharpAsync
{
    public static Task<T> StartAsTask<T>(FSharpAsync<T> computation, TaskCreationOptions? options = null, CancellationToken? cancellationToken = null)
        => Task.FromResult(computation.Result);
}