namespace Fixie
{
    using System;
    using System.Reflection;

    public class SkipBehavior
    {
        readonly Func<MethodInfo, bool> skipMethod;
        readonly Func<MethodInfo, string> getSkipReason;

        public SkipBehavior(Func<MethodInfo, bool> skipMethod, Func<MethodInfo, string> getSkipReason)
        {
            this.skipMethod = skipMethod;
            this.getSkipReason = getSkipReason;
        }

        public bool SkipMethod(MethodInfo testMethod)
            => skipMethod(testMethod);

        public string GetSkipReason(MethodInfo testMethod)
            => getSkipReason(testMethod);
    }
}