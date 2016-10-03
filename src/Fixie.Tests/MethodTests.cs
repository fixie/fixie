namespace Fixie.Tests
{
    using Assertions;

    public class MethodTests
    {
        public void CanRepresentMethodDeclaredInChildClass()
        {
            var methodInfo = typeof(ChildClass).GetInstanceMethod("MethodDefinedWithinChildClass");

            var actual = new Method(typeof(ChildClass), methodInfo);
            actual.Class.ShouldEqual(typeof(ChildClass));
            actual.MethodInfo.ShouldEqual(methodInfo);
        }

        public void CanRepresentMethodDeclaredInParentClass()
        {
            var methodInfo = typeof(ParentClass).GetInstanceMethod("MethodDefinedWithinParentClass");

            var actual = new Method(typeof(ParentClass), methodInfo);
            actual.Class.ShouldEqual(typeof(ParentClass));
            actual.MethodInfo.ShouldEqual(methodInfo);
        }

        public void CanRepresentMethodInheritedByChildClass()
        {
            var methodInfo = typeof(ChildClass).GetInstanceMethod("MethodDefinedWithinParentClass");

            var actual = new Method(typeof(ChildClass), methodInfo);
            actual.Class.ShouldEqual(typeof(ChildClass));
            actual.MethodInfo.ShouldEqual(methodInfo);
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