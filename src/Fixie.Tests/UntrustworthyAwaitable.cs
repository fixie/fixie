using System;
using System.Runtime.CompilerServices;

namespace Fixie.Tests;

// This provides enough structural support for the async/await keywords
// to be satisfied at compile time, but is not expected to behave well.

[AsyncMethodBuilder(typeof(UntrustworthyMethodBuilder))]
class UntrustworthyAwaitable
{
    public UntrustworthyAwaiter GetAwaiter()
        => new UntrustworthyAwaiter();
}

class UntrustworthyAwaiter : INotifyCompletion
{
    public void OnCompleted(Action completion)
        => throw new ShouldBeUnreachableException();
}

class UntrustworthyMethodBuilder
{
    public static UntrustworthyMethodBuilder Create()
        => new UntrustworthyMethodBuilder();

    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine { }

    public void SetStateMachine(IAsyncStateMachine stateMachine) { }

    public void SetException(Exception exception) { }

    public void SetResult() { }

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine { }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine { }
        
    public UntrustworthyAwaitable Task
        => new UntrustworthyAwaitable();
}