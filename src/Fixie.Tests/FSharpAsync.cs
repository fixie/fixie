// Test stub for F#'s Async<'T> such that we can
// reflect on it in the same way as we can against
// the real implementation during F# test runs.

namespace Microsoft.FSharp.Control;

using System;
using System.Threading;
using System.Threading.Tasks;

public class FSharpAsync<TResult>
{
    readonly Func<TResult> getResult;

    public FSharpAsync(TResult result) : this(() => result) { }

    public FSharpAsync(Func<TResult> getResult) => this.getResult = getResult;

    public TResult Result => getResult();
}

public class FSharpAsync
{
    public static Task<T> StartAsTask<T>(FSharpAsync<T> computation, TaskCreationOptions? options = null, CancellationToken? cancellationToken = null)
        => Task.FromResult(computation.Result);
}