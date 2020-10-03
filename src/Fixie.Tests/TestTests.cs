namespace Fixie.Tests
{
    using Assertions;
    using static Utility;

    public class TestTests
    {
        public void CanRepresentMethodsDeclaredInChildClasses()
        {
            AssertTest(
                Test<ChildClass>("MethodDefinedWithinChildClass"),
                "Fixie.Tests.TestTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinChildClass");
        }

        public void CanRepresentMethodsDeclaredInParentClasses()
        {
            AssertTest(
                Test<ParentClass>("MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestTests+ParentClass.MethodDefinedWithinParentClass");
        }

        public void CanRepresentParentMethodsInheritedByChildClasses()
        {
            AssertTest(
                Test<ChildClass>("MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinParentClass");
        }

        public void CanParseFromFullNameStrings()
        {
            AssertTest(
                new Test("Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinChildClass"),
                "Fixie.Tests.TestTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinChildClass");

            AssertTest(
                new Test("Fixie.Tests.TestTests+ParentClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestTests+ParentClass.MethodDefinedWithinParentClass");

            AssertTest(
                new Test("Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinParentClass");
        }

        public void CanBeConstructedFromTrustedClassNameAndMethodName()
        {
            AssertTest(
                new Test(FullName<ChildClass>(), "MethodDefinedWithinChildClass"),
                "Fixie.Tests.TestTests+ChildClass",
                "MethodDefinedWithinChildClass",
                "Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinChildClass");

            AssertTest(
                new Test(FullName<ParentClass>(), "MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestTests+ParentClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestTests+ParentClass.MethodDefinedWithinParentClass");

            AssertTest(
                new Test(FullName<ChildClass>(), "MethodDefinedWithinParentClass"),
                "Fixie.Tests.TestTests+ChildClass",
                "MethodDefinedWithinParentClass",
                "Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinParentClass");
        }

        public void CanDetermineWhetherItMatchesTheGivenPattern()
        {
            var childClassChildMethod = new Test("Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinChildClass");
            var parentClassParentMethod = new Test("Fixie.Tests.TestTests+ParentClass.MethodDefinedWithinParentClass");
            var childClassParentMethod = new Test("Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinParentClass");

            foreach (var test in new[] {childClassChildMethod, parentClassParentMethod, childClassParentMethod})
            {
                // The empty pattern matches everything, as it is essentially an empty substring match.
                test.Matches("").ShouldBe(true);
                
                // Clear mismatch.
                test.Matches("ZZZ").ShouldBe(false);
                
                // Perfect match on full name.
                test.Matches(test.Name).ShouldBe(true);
                
                // Substring match on full name.
                test.Matches("Fixie.Tests.TestTests+").ShouldBe(true);
                test.Matches("Class.MethodDefinedWithin").ShouldBe(true);

                // Explicit wildcard matches everything.
                test.Matches("*").ShouldBe(true);
                test.Matches("F*Tests+").ShouldBe(true);
                test.Matches("Cla*hodDef*thin").ShouldBe(true);

                // Implicit [a-z]* between upper-case and non-lower-case characters.
                test.Matches("T+").ShouldBe(true);
                test.Matches("F.T.TT+").ShouldBe(true);
                test.Matches("C.MDW").ShouldBe(true);

                // Explicit lower-case can prevent the implied wildcard.
                test.Matches("Te+").ShouldBe(false);
                test.Matches("F.T.TTe+").ShouldBe(false);
                test.Matches("Cl.MDW").ShouldBe(false);
            }

            childClassChildMethod.Matches("C").ShouldBe(true);
            parentClassParentMethod.Matches("C").ShouldBe(true);
            childClassParentMethod.Matches("C").ShouldBe(true);

            childClassChildMethod.Matches("CC").ShouldBe(true);
            parentClassParentMethod.Matches("CC").ShouldBe(false);
            childClassParentMethod.Matches("CC").ShouldBe(true);

            childClassChildMethod.Matches("CC.MDW").ShouldBe(true);
            parentClassParentMethod.Matches("CC.MDW").ShouldBe(false);
            childClassParentMethod.Matches("CC.MDW").ShouldBe(true);

            childClassChildMethod.Matches("C.MDW").ShouldBe(true);
            parentClassParentMethod.Matches("C.MDW").ShouldBe(true);
            childClassParentMethod.Matches("C.MDW").ShouldBe(true);

            childClassChildMethod.Matches("C.MDWC").ShouldBe(true);
            parentClassParentMethod.Matches("C.MDWC").ShouldBe(false);
            childClassParentMethod.Matches("C.MDWC").ShouldBe(false);

            childClassChildMethod.Matches("C.MDWP").ShouldBe(false);
            parentClassParentMethod.Matches("C.MDWP").ShouldBe(true);
            childClassParentMethod.Matches("C.MDWP").ShouldBe(true);

            childClassChildMethod.Matches("ChildClass").ShouldBe(true);
            parentClassParentMethod.Matches("ChildClass").ShouldBe(false);
            childClassParentMethod.Matches("ChildClass").ShouldBe(true);

            childClassChildMethod.Matches("ChildClass.MethodDefinedWithin").ShouldBe(true);
            parentClassParentMethod.Matches("ChildClass.MethodDefinedWithin").ShouldBe(false);
            childClassParentMethod.Matches("ChildClass.MethodDefinedWithin").ShouldBe(true);

            childClassChildMethod.Matches("ChildClass.MethodDefinedWithinP").ShouldBe(false);
            parentClassParentMethod.Matches("ChildClass.MethodDefinedWithinP").ShouldBe(false);
            childClassParentMethod.Matches("ChildClass.MethodDefinedWithinP").ShouldBe(true);

            childClassChildMethod.Matches("ChildClass.Met*odDefinedWithin").ShouldBe(true);
            parentClassParentMethod.Matches("ChildClass.Met*odDefinedWithin").ShouldBe(false);
            childClassParentMethod.Matches("ChildClass.Met*odDefinedWithin").ShouldBe(true);

            childClassChildMethod.Matches("ChildClass.Met*odDefinedWithinP").ShouldBe(false);
            parentClassParentMethod.Matches("ChildClass.Met*odDefinedWithinP").ShouldBe(false);
            childClassParentMethod.Matches("ChildClass.Met*odDefinedWithinP").ShouldBe(true);

            childClassChildMethod.Matches("*M*e*t*h*o*d*D*e*f*i*n*e*d*W*i*t*h*i*n*P").ShouldBe(false);
            parentClassParentMethod.Matches("*M*e*t*h*o*d*D*e*f*i*n*e*d*W*i*t*h*i*n*P").ShouldBe(true);
            childClassParentMethod.Matches("*M*e*t*h*o*d*D*e*f*i*n*e*d*W*i*t*h*i*n*P").ShouldBe(true);

            childClassChildMethod.Matches("C*.M*D*W*P").ShouldBe(false);
            parentClassParentMethod.Matches("C*.M*D*W*P").ShouldBe(true);
            childClassParentMethod.Matches("C*.M*D*W*P").ShouldBe(true);
        }

        static void AssertTest(Test actual, string expectedClass, string expectedMethod, string expectedName)
        {
            actual.Class.ShouldBe(expectedClass);
            actual.Method.ShouldBe(expectedMethod);
            actual.Name.ShouldBe(expectedName);
        }

        static Test Test<TTestClass>(string method)
            => new Test(typeof(TTestClass).GetInstanceMethod(method));

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