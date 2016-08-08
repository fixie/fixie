namespace Fixie.Cli
{
    using System.Collections.Generic;
    using System.Linq;

    public class ParseResult<T>
    {
        public ParseResult(T model, IReadOnlyCollection<string> extraArguments, IReadOnlyCollection<string> errors)
        {
            Model = model;
            ExtraArguments = extraArguments;
            Errors = errors.ToArray();
        }

        public T Model { get; }
        public IReadOnlyCollection<string> ExtraArguments { get; } 
        public IReadOnlyCollection<string> Errors { get; } 
    }
}