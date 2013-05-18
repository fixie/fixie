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

        public void CanDetectClassAttributes()
        {
            typeof(AttributeSample).Has<InheritedAttribute>().ShouldBeFalse();
            typeof(AttributeSample).Has<NonInheritedAttribute>().ShouldBeTrue();
            typeof(AttributeSample).Has<SerializableAttribute>().ShouldBeFalse();

            typeof(AttributeSample).HasOrInherits<InheritedAttribute>().ShouldBeTrue();
            typeof(AttributeSample).HasOrInherits<NonInheritedAttribute>().ShouldBeTrue();
            typeof(AttributeSample).HasOrInherits<SerializableAttribute>().ShouldBeFalse();
        }

        public void CanDetectMethodAttributes()
        {
            Method<AttributeSample>("AttributeOnBaseDeclaration").Has<SampleMethodAttribute>().ShouldBeFalse();
            Method<AttributeSample>("AttributeOnOverrideDeclaration").Has<SampleMethodAttribute>().ShouldBeTrue();
            Method<AttributeSample>("NoAttrribute").Has<SampleMethodAttribute>().ShouldBeFalse();

            Method<AttributeSample>("AttributeOnBaseDeclaration").HasOrInherits<SampleMethodAttribute>().ShouldBeTrue();
            Method<AttributeSample>("AttributeOnOverrideDeclaration").HasOrInherits<SampleMethodAttribute>().ShouldBeTrue();
            Method<AttributeSample>("NoAttrribute").HasOrInherits<SampleMethodAttribute>().ShouldBeFalse();
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

        void ReturnsVoid() { }
        int ReturnsInt() { return 0; }
        async Task Async() { await Task.Run(() => { }); }

        class SampleMethodAttribute : Attribute { }
        class InheritedAttribute : Attribute { }
        class NonInheritedAttribute : Attribute { }

        [Inherited]
        abstract class AttributeSampleBase
        {
            [SampleMethod]
            public virtual void AttributeOnBaseDeclaration() { }
            public virtual void AttributeOnOverrideDeclaration() { }
            public virtual void NoAttrribute() { }
        }

        [NonInheritedAttribute]
        class AttributeSample : AttributeSampleBase
        {
            public override void AttributeOnBaseDeclaration() { }
            [SampleMethod]
            public override void AttributeOnOverrideDeclaration() { }
            public override void NoAttrribute() { }
        }

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