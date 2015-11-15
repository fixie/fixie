using System.Collections.Generic;
using System.Text;
using Fixie.Execution;

namespace Fixie.Tests
{
    public class StubListener : Listener,
        IHandler<AssemblyStarted>,
        IHandler<SkipResult>,
        IHandler<PassResult>,
        IHandler<FailResult>,
        IHandler<AssemblyCompleted>
    {
        readonly List<string> log = new List<string>();

        public void Handle(AssemblyStarted message)
        {
        }

        public void Handle(SkipResult result)
        {
            var optionalReason = result.SkipReason == null ? "." : ": " + result.SkipReason;
            log.Add($"{result.Name} skipped{optionalReason}");
        }

        public void Handle(PassResult result)
        {
            log.Add($"{result.Name} passed.");
        }

        public void Handle(FailResult result)
        {
            var entry = new StringBuilder();

            var primaryException = result.Exceptions.PrimaryException;

            entry.Append($"{result.Name} failed: {primaryException.Message}");

            var walk = primaryException;
            while (walk.InnerException != null)
            {
                walk = walk.InnerException;
                entry.AppendLine();
                entry.Append($"    Inner Exception: {walk.Message}");
            }

            foreach (var secondaryException in result.Exceptions.SecondaryExceptions)
            {
                entry.AppendLine();
                entry.Append($"    Secondary Failure: {secondaryException.Message}");

                walk = secondaryException;
                while (walk.InnerException != null)
                {
                    walk = walk.InnerException;
                    entry.AppendLine();
                    entry.Append($"        Inner Exception: {walk.Message}");
                }
            }

            log.Add(entry.ToString());
        }

        public void Handle(AssemblyCompleted message)
        {
        }

        public IEnumerable<string> Entries => log;
    }
}