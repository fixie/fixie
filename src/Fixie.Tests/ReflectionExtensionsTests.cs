namespace Fixie.Tests
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Should;

    public class ReflectionExtensionsTests
    {
        public void CanDetectVoidReturnType()
        {
            Method("ReturnsVoid").IsVoid().ShouldBeTrue();
            Method("ReturnsInt").IsVoid().ShouldBeFalse();
        }

        public void CanDetectClassAttributes()
        {
            typeof(AttributeSample).Has<InheritedAttribute>().ShouldBeFalse();
            typeof(AttributeSample).Has<NonInheritedAttribute>().ShouldBeTrue();
            typeof(AttributeSample).Has<AttributeUsageAttribute>().ShouldBeFalse();

            typeof(AttributeSample).HasOrInherits<InheritedAttribute>().ShouldBeTrue();
            typeof(AttributeSample).HasOrInherits<NonInheritedAttribute>().ShouldBeTrue();
            typeof(AttributeSample).HasOrInherits<AttributeUsageAttribute>().ShouldBeFalse();
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
            Method("ReturnsVoid").IsAsync().ShouldBeFalse();
            Method("ReturnsInt").IsAsync().ShouldBeFalse();
            Method("Async").IsAsync().ShouldBeTrue();
        }

        public void CanDetectWhetherMethodIsDispose()
        {
            Method("ReturnsVoid").IsDispose().ShouldBeFalse();
            Method("ReturnsInt").IsDispose().ShouldBeFalse();
            Method("Async").IsDispose().ShouldBeFalse();
            Method<NonDisposableWithDisposeMethod>("Dispose").IsDispose().ShouldBeFalse();
            MethodBySignature<Disposable>(typeof(void), "Dispose", typeof(bool)).IsDispose().ShouldBeFalse();
            MethodBySignature<Disposable>(typeof(void), "Dispose").IsDispose().ShouldBeTrue();
        }

        public void CanDetectWhetherMethodHasSignature()
        {
            var trivial = MethodBySignature<Signatures>(typeof(void), "Trivial");
            trivial.HasSignature(typeof(int), "Trivial").ShouldBeFalse();
            trivial.HasSignature(typeof(void), "!").ShouldBeFalse();
            trivial.HasSignature(typeof(void), "Trivial", typeof(int)).ShouldBeFalse();
            trivial.HasSignature(typeof(void), "Trivial").ShouldBeTrue();

            var singleParam = MethodBySignature<Signatures>(typeof(int), "Params", typeof(string));
            singleParam.HasSignature(typeof(int), "Params", typeof(int)).ShouldBeFalse();
            singleParam.HasSignature(typeof(int), "Params", typeof(string), typeof(int)).ShouldBeFalse();
            singleParam.HasSignature(typeof(int), "Params", typeof(string)).ShouldBeTrue();

            var multipleParam = MethodBySignature<Signatures>(typeof(string), "Params", typeof(string), typeof(int));
            multipleParam.HasSignature(typeof(string), "Params", typeof(string), typeof(string)).ShouldBeFalse();
            multipleParam.HasSignature(typeof(string), "Params", typeof(string), typeof(int), typeof(int)).ShouldBeFalse();
            multipleParam.HasSignature(typeof(string), "Params", typeof(string), typeof(int)).ShouldBeTrue();
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

        class Signatures
        {
            void Trivial() { }
            int Params(string s) { return 0; }
            string Params(string s, int x) { return ""; }
        }

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

        [NonInherited]
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
            return typeof(T).GetInstanceMethod(name);
        }

        private static MethodInfo MethodBySignature<T>(Type returnType, string name, params Type[] parameterTypes)
        {
            return typeof(T).GetInstanceMethods().Single(m => m.HasSignature(returnType, name, parameterTypes));
        }
    }
}