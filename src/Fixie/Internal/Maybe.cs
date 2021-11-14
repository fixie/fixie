namespace Fixie.Internal;

using System;
using System.Diagnostics.CodeAnalysis;

static class Maybe
{
    public static bool Try<T>(Func<T> create, [NotNullWhen(true)] out T output)
    {
        output = create();

        return output != null;
    }

    public static bool Try<TInput, TOutput>(Func<TInput, TOutput> create, TInput input, [NotNullWhen(true)] out TOutput output)
    {
        output = create(input);

        return output != null;
    }
        
    public static bool Try<TInput1, TInput2, TOutput>(Func<TInput1, TInput2, TOutput> create, TInput1 input1, TInput2 input2, [NotNullWhen(true)] out TOutput output)
    {
        output = create(input1, input2);

        return output != null;
    }
}