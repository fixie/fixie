using System.Reflection;
using System.Threading.Tasks;
using Should;
using Xunit;

namespace Fixie.Tests
{
    public class ReflectionExtensionsTests
    {
        [Fact]
        public void CanDetectVoidReturnType()
        {
            Method("ReturnsVoid").Void().ShouldBeTrue();
            Method("ReturnsInt").Void().ShouldBeFalse();
        }

        [Fact]
        public void CanDetectMethodAttributes()
        {
            Method("ReturnsVoid").Has<FactAttribute>().ShouldBeFalse();
            Method("ReturnsInt").Has<FactAttribute>().ShouldBeFalse();
            Method("CanDetectMethodAttributes").Has<FactAttribute>().ShouldBeTrue();
        }

        [Fact]
        public void CanDetectAsyncDeclarations()
        {
            Method("ReturnsVoid").Async().ShouldBeFalse();
            Method("ReturnsInt").Async().ShouldBeFalse();
            Method("Async").Async().ShouldBeTrue();
        }

        [Fact]
        public void CanDetectWhetherTypeIsWithinNamespace()
        {
            var opCode = typeof(System.Reflection.Emit.OpCode);

            opCode.IsInNamespace(null).ShouldBeFalse();
            opCode.IsInNamespace("System").ShouldBeTrue();
            opCode.IsInNamespace("Sys").ShouldBeFalse();
            opCode.IsInNamespace("System.").ShouldBeFalse();

            opCode.IsInNamespace("System.Reflection").ShouldBeTrue();
            opCode.IsInNamespace("System.Reflection.Emit").ShouldBeTrue();
            opCode.IsInNamespace("System.Reflection.Emit.OpCode").ShouldBeFalse();
            opCode.IsInNamespace("System.Reflection.Typo").ShouldBeFalse();
        }

        void ReturnsVoid() { }
        int ReturnsInt() { return 0; }
        async Task Async() { await Task.Run(() => { }); }

        static MethodInfo Method(string name)
        {
            return typeof(ReflectionExtensionsTests).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
    }
}