using Fixie.Discovery;
using Should;

namespace Fixie.Tests.Discovery
{
    public class MethodGroupTests
    {
        public void CanConstructMethodGroupFromMethodInfoWithRespectToTheReflectedType()
        {
            var methodDeclaredInChildClass = typeof(ChildClass).GetInstanceMethod("MethodDefinedWithinChildClass");
            var methodDeclaredInParentClass = typeof(ParentClass).GetInstanceMethod("MethodDefinedWithinParentClass");
            var parentMethodInheritedByChildClass = typeof(ChildClass).GetInstanceMethod("MethodDefinedWithinParentClass");

            AssertMethodGroup(
                new MethodGroup(methodDeclaredInChildClass),
                "Fixie.Tests.Discovery.MethodGroupTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.Discovery.MethodGroupTests+ChildClass.MethodDefinedWithinChildClass");

            AssertMethodGroup(
                new MethodGroup(methodDeclaredInParentClass),
                "Fixie.Tests.Discovery.MethodGroupTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.Discovery.MethodGroupTests+ParentClass.MethodDefinedWithinParentClass");

            AssertMethodGroup(
                new MethodGroup(parentMethodInheritedByChildClass),
                "Fixie.Tests.Discovery.MethodGroupTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.Discovery.MethodGroupTests+ChildClass.MethodDefinedWithinParentClass");
        }

        public void CanParseFromFullNameStrings()
        {
            AssertMethodGroup(
                new MethodGroup("Fixie.Tests.Discovery.MethodGroupTests+ChildClass.MethodDefinedWithinChildClass"),
                "Fixie.Tests.Discovery.MethodGroupTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.Discovery.MethodGroupTests+ChildClass.MethodDefinedWithinChildClass");

            AssertMethodGroup(
                new MethodGroup("Fixie.Tests.Discovery.MethodGroupTests+ParentClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.Discovery.MethodGroupTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.Discovery.MethodGroupTests+ParentClass.MethodDefinedWithinParentClass");

            AssertMethodGroup(
                new MethodGroup("Fixie.Tests.Discovery.MethodGroupTests+ChildClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.Discovery.MethodGroupTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.Discovery.MethodGroupTests+ChildClass.MethodDefinedWithinParentClass");
        }

        static void AssertMethodGroup(MethodGroup actual, string expectedClass, string expectedMethod, string expectedFullName)
        {
            actual.Class.ShouldEqual(expectedClass);
            actual.Method.ShouldEqual(expectedMethod);
            actual.FullName.ShouldEqual(expectedFullName);
        }

        class ParentClass
        {
            public void MethodDefinedWithinParentClass()
            {
            }
        }

        class ChildClass : ParentClass
        {
            public void MethodDefinedWithinChildClass()
            {
            }
        }
    }
}