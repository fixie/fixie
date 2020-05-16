namespace Fixie.Internal
{
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
    }
}
