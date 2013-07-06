using System.Linq;
using System.Reflection;

namespace Fixie
{
    public class RunContext
    {
        public RunContext(Assembly assembly, ILookup<string, string> options)
            : this(assembly, options, null) { }

        public RunContext(Assembly assembly, ILookup<string, string> options, MemberInfo targetMember)
        {
            Assembly = assembly;
            Options = options;
            TargetMember = targetMember;
        }

        public Assembly Assembly { get; private set; }
        public ILookup<string, string> Options { get; private set; }
        public MemberInfo TargetMember { get; private set; }
    }
}