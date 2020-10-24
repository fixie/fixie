namespace Fixie.Tests
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
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
            Method<AttributeSample>("NoAttribute").Has<SampleMethodAttribute>().ShouldBe(false);
        }

        public void CanDetectAndObtainAttributeWhenOneTimeUseAttributeIsPresent()
        {
            //Zero Matches
            var hasMissingAttribute =  typeof(AttributeSample).Has<AttributeUsageAttribute>(out var missingAttribute);
            hasMissingAttribute.ShouldBe(false);
            missingAttribute.ShouldBe(null);

            //Single Match, Inherited
            var hasInheritedAttribute = typeof(AttributeSample).Has<InheritedAttribute>(out var inheritedAttribute);
            hasInheritedAttribute.ShouldBe(true);
            inheritedAttribute.ShouldBe<InheritedAttribute>();

            //Single Match, Not Inherited
            var hasNonInheritedAttribute = typeof(AttributeSample).Has<NonInheritedAttribute>(out var nonInheritedAttribute);
            hasNonInheritedAttribute.ShouldBe(true);
            nonInheritedAttribute.ShouldBe<NonInheritedAttribute>();

            //Ambiguous Match
            Action attemptAmbiguousAttributeLookup = () => typeof(AttributeSample).Has<AmbiguouslyMultipleAttribute>(out _);
            attemptAmbiguousAttributeLookup.ShouldThrow<AmbiguousMatchException>("Multiple custom attributes of the same type found.");
        }

        public async Task CanDisposeDisposablesAndAsyncDisposables()
        {
            var disposeable = new Disposable();
            var disposeButNotDisposable = new DisposeButNotDisposable();
            var notDisposable = new NotDisposable();
            object? nullObject = null;

            disposeable.DisposeAsyncInvoked.ShouldBe(false);
            disposeable.DisposeInvoked.ShouldBe(false);
            disposeButNotDisposable.DisposeAsyncInvoked.ShouldBe(false);
            disposeButNotDisposable.DisposeInvoked.ShouldBe(false);
            notDisposable.Invoked.ShouldBe(false);

            await ((object)disposeable).DisposeIfApplicableAsync();
            await ((object)disposeButNotDisposable).DisposeIfApplicableAsync();
            await notDisposable.DisposeIfApplicableAsync();
            await nullObject.DisposeIfApplicableAsync();

            disposeable.DisposeAsyncInvoked.ShouldBe(true);
            disposeable.DisposeInvoked.ShouldBe(true);
            disposeButNotDisposable.DisposeAsyncInvoked.ShouldBe(false);
            disposeButNotDisposable.DisposeInvoked.ShouldBe(false);
            notDisposable.Invoked.ShouldBe(false);
        }

        void ReturnsVoid() { }
        int ReturnsInt() { return 0; }

        class SampleMethodAttribute : Attribute { }
        class InheritedAttribute : Attribute { }
        class NonInheritedAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        class AmbiguouslyMultipleAttribute : Attribute { }

        static class StaticClass { }
        abstract class AbstractClass { }
        class ConcreteClass { }
        sealed class SealedConcreteClass { }

        [Inherited]
        [AmbiguouslyMultiple]
        [AmbiguouslyMultiple]
        abstract class AttributeSampleBase
        {
            [SampleMethod]
            public virtual void AttributeOnBaseDeclaration() { }
            public virtual void AttributeOnOverrideDeclaration() { }
            public virtual void NoAttribute() { }
        }

        [NonInherited]
        class AttributeSample : AttributeSampleBase
        {
            public override void AttributeOnBaseDeclaration() { }
            [SampleMethod]
            public override void AttributeOnOverrideDeclaration() { }
            public override void NoAttribute() { }
        }

        static MethodInfo Method(string name)
            => Method<ReflectionExtensionsTests>(name);

        static MethodInfo Method<T>(string name)
            => typeof(T).GetInstanceMethod(name);

        class Disposable : IAsyncDisposable, IDisposable
        {
            public bool DisposeAsyncInvoked { get; private set; }
            public bool DisposeInvoked { get; private set; }

            public ValueTask DisposeAsync()
            {
                DisposeAsyncInvoked = true;
                return default;
            }

            public void Dispose() => DisposeInvoked = true;
        }

        class DisposeButNotDisposable
        {
            public bool DisposeAsyncInvoked { get; private set; }
            public bool DisposeInvoked { get; private set; }

            public ValueTask DisposeAsync()
            {
                DisposeAsyncInvoked = true;
                return default;
            }

            public void Dispose() => DisposeInvoked = true;
        }

        class NotDisposable
        {
            public bool Invoked { get; private set; }
        }
    }
}