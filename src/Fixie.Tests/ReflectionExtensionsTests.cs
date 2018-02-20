namespace Fixie.Tests
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Assertions;

    public class ReflectionExtensionsTests
    {
        public void CanDetermineTheTypeNameOfAnyObject()
        {
            5.TypeName().ShouldEqual("System.Int32");
            "".TypeName().ShouldEqual("System.String");
            ((string) null).TypeName().ShouldBeNull();
        }

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
    }
}