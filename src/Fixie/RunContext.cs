using System.Reflection;

namespace Fixie
{
    public class RunContext
    {
        public RunContext(Assembly assembly)
            : this(assembly, null) { }

        public RunContext(Assembly assembly, MemberInfo targetMember)
        {
            Assembly = assembly;
            TargetMember = targetMember;
        }

        public Assembly Assembly { get; private set; }
        public MemberInfo TargetMember { get; private set; }
    }
}