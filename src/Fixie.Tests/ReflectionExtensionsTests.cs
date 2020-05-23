namespace Fixie.Tests
{
    using System;
    using System.Reflection;
    using Assertions;

    public class ReflectionExtensionsTests
    {
        public void CanDetectVoidReturnType()
        {
            Method("ReturnsVoid").IsVoid().ShouldBe(true);
            Method("ReturnsInt").IsVoid().ShouldBe(false);
        }

        public void CanDetectStaticTypes()
        {
            typeof(StaticClass).IsStatic().ShouldBe(true);
            typeof(AbstractClass).IsStatic().ShouldBe(false);
            typeof(ConcreteClass).IsStatic().ShouldBe(false);
            typeof(SealedConcreteClass).IsStatic().ShouldBe(false);
        }

        public void CanDetectAttributes()
        {
            typeof(AttributeSample).Has<InheritedAttribute>().ShouldBe(true);
            typeof(AttributeSample).Has<NonInheritedAttribute>().ShouldBe(true);
            typeof(AttributeSample).Has<AttributeUsageAttribute>().ShouldBe(false);

            Method<AttributeSample>("AttributeOnBaseDeclaration").Has<SampleMethodAttribute>().ShouldBe(true);
            Method<AttributeSample>("AttributeOnOverrideDeclaration").Has<SampleMethodAttribute>().ShouldBe(true);
            Method<AttributeSample>("NoAttrribute").Has<SampleMethodAttribute>().ShouldBe(false);
        }

        public void CanDisposeDisposables()
        {
            var disposeable = new Disposable();
            var disposeButNotDisposable = new DisposeButNotDisposable();
            var notDisposable = new NotDisposable();
            object? nullObject = null;

            disposeable.Invoked.ShouldBe(false);
            disposeButNotDisposable.Invoked.ShouldBe(false);
            notDisposable.Invoked.ShouldBe(false);

            ((object)disposeable).Dispose();
            ((object)disposeButNotDisposable).Dispose();
            notDisposable.Dispose();
            nullObject.Dispose();

            disposeable.Invoked.ShouldBe(true);
            disposeButNotDisposable.Invoked.ShouldBe(false);
            notDisposable.Invoked.ShouldBe(false);
        }

        void ReturnsVoid() { }
        int ReturnsInt() { return 0; }

        class SampleMethodAttribute : Attribute { }
        class InheritedAttribute : Attribute { }
        class NonInheritedAttribute : Attribute { }

        static class StaticClass { }
        abstract class AbstractClass { }
        class ConcreteClass { }
        sealed class SealedConcreteClass { }

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

        static MethodInfo Method(string name)
            => Method<ReflectionExtensionsTests>(name);

        static MethodInfo Method<T>(string name)
            => typeof(T).GetInstanceMethod(name);

        class Disposable : IDisposable
        {
            public bool Invoked { get; private set; }
            public void Dispose() => Invoked = true;
        }

        class DisposeButNotDisposable
        {
            public bool Invoked { get; private set; }
            public void Dispose() => Invoked = true;
        }

        class NotDisposable
        {
            public bool Invoked { get; private set; }
        }
    }
}