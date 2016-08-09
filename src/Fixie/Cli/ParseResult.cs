namespace Fixie.Cli
{
    using System.Collections.Generic;

    public class ParseResult<T>
    {
        public ParseResult(T model, IReadOnlyCollection<string> extraArguments)
        {
            Model = model;
            ExtraArguments = extraArguments;
        }

        public T Model { get; }
        public IReadOnlyCollection<string> ExtraArguments { get; } 
    }
}