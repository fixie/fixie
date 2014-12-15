using System.Reflection;

namespace Fixie
{
    public class RunContext
    {
        public RunContext(Assembly assembly, Options options)
            : this(assembly, options, null) { }

        public RunContext(Assembly assembly, Options options, MemberInfo targetMember)
        {
            Assembly = assembly;
            Options = options;
            TargetMember = targetMember;
        }

        public Assembly Assembly { get; private set; }
        public Options Options { get; private set; }
        public MemberInfo TargetMember { get; private set; }
    }
}