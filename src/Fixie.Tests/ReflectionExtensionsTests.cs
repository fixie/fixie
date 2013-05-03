using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Should;

namespace Fixie.Tests
{
    public class ReflectionExtensionsTests
    {
        const BindingFlags InstanceMethods = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

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

        public void CanDetectWhetherMethodIsDispose()
        {
            Method("ReturnsVoid").IsDispose().ShouldBeFalse();
            Method("ReturnsInt").IsDispose().ShouldBeFalse();
            Method("Async").IsDispose().ShouldBeFalse();
            Method<NonDisposableWithDisposeMethod>("Dispose").IsDispose().ShouldBeFalse();
            MethodBySignature<Disposable>("Dispose", typeof(void), typeof(bool)).IsDispose().ShouldBeFalse();
            MethodBySignature<Disposable>("Dispose", typeof(void)).IsDispose().ShouldBeTrue();
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

        class NonDisposableWithDisposeMethod
        {
            public void Dispose() { }
        }

        class Disposable : NonDisposableWithDisposeMethod, IDisposable
        {
            public void Dispose(bool disposing) { }
        }

        static MethodInfo Method(string name)
        {
            return Method<ReflectionExtensionsTests>(name);
        }

        static MethodInfo Method<T>(string name)
        {
            return typeof(T).GetMethod(name, InstanceMethods);
        }

        private static MethodInfo MethodBySignature<T>(string name, Type returnType, params Type[] parameterTypes)
        {
            return typeof(T).GetMethods(InstanceMethods).Single(m =>
            {
                if (m.Name != name) return false;
                if (m.ReturnType != returnType) return false;

                var parameters = m.GetParameters();

                if (parameters.Length != parameterTypes.Length)
                    return false;

                for (int i = 0; i < parameterTypes.Length; i++)
                    if (parameters[i].ParameterType != parameterTypes[i])
                        return false;

                return true;
            });
        }
    }
}