using System;
using System.Reflection;
using System.Threading.Tasks;
using Should;

namespace Fixie.Tests
{
    public class ReflectionExtensionsTests
    {
        public void CanDetectVoidReturnType()
        {
            Method("ReturnsVoid").Void().ShouldBeTrue();
            Method("ReturnsInt").Void().ShouldBeFalse();
        }

        public void CanDetectMethodAttributes()
        {
            Method("ReturnsVoid").Has<SampleAttribute>().ShouldBeFalse();
            Method("ReturnsInt").Has<SampleAttribute>().ShouldBeFalse();
            Method("Async").Has<SampleAttribute>().ShouldBeTrue();
        }

        public void CanDetectAsyncDeclarations()
        {
            Method("ReturnsVoid").Async().ShouldBeFalse();
            Method("ReturnsInt").Async().ShouldBeFalse();
            Method("Async").Async().ShouldBeTrue();
        }

        public void CanDetectWhetherTypeIsWithinNamespace()
        {
            var opCode = typeof(System.Reflection.Emit.OpCode);

            opCode.IsInNamespace(null).ShouldBeFalse();
            opCode.IsInNamespace("").ShouldBeFalse();
            opCode.IsInNamespace("System").ShouldBeTrue();
            opCode.IsInNamespace("Sys").ShouldBeFalse();
            opCode.IsInNamespace("System.").ShouldBeFalse();

            opCode.IsInNamespace("System.Reflection").ShouldBeTrue();
            opCode.IsInNamespace("System.Reflection.Emit").ShouldBeTrue();
            opCode.IsInNamespace("System.Reflection.Emit.OpCode").ShouldBeFalse();
            opCode.IsInNamespace("System.Reflection.Typo").ShouldBeFalse();
        }

        class SampleAttribute : Attribute { }

        void ReturnsVoid() { }
        int ReturnsInt() { return 0; }
        [Sample] async Task Async() { await Task.Run(() => { }); }

        static MethodInfo Method(string name)
        {
            return typeof(ReflectionExtensionsTests).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
    }
}