namespace Fixie.Tests
{
    using Assertions;

    public class TestPatternTests
    {
        public void CanDetermineWhetherTestsMatchTheGivenPattern()
        {
            var childClassChildMethod = new Test("Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinChildClass");
            var parentClassParentMethod = new Test("Fixie.Tests.TestTests+ParentClass.MethodDefinedWithinParentClass");
            var childClassParentMethod = new Test("Fixie.Tests.TestTests+ChildClass.MethodDefinedWithinParentClass");

            foreach (var test in new[] {childClassChildMethod, parentClassParentMethod, childClassParentMethod})
            {
                // The empty pattern matches everything, as it is essentially an empty substring match.
                new TestPattern("").Matches(test).ShouldBe(true);
                
                // Clear mismatch.
                new TestPattern("ZZZ").Matches(test).ShouldBe(false);
                
                // Perfect match on full name.
                new TestPattern(test.Name).Matches(test).ShouldBe(true);
                
                // Substring match on full name.
                new TestPattern("Fixie.Tests.TestTests+").Matches(test).ShouldBe(true);
                new TestPattern("Class.MethodDefinedWithin").Matches(test).ShouldBe(true);

                // Explicit wildcard matches everything.
                new TestPattern("*").Matches(test).ShouldBe(true);
                new TestPattern("F*Tests+").Matches(test).ShouldBe(true);
                new TestPattern("Cla*hodDef*thin").Matches(test).ShouldBe(true);

                // Implicit [a-z]* between upper-case and non-lower-case characters.
                new TestPattern("T+").Matches(test).ShouldBe(true);
                new TestPattern("F.T.TT+").Matches(test).ShouldBe(true);
                new TestPattern("C.MDW").Matches(test).ShouldBe(true);

                // Explicit lower-case can prevent the implied wildcard.
                new TestPattern("Te+").Matches(test).ShouldBe(false);
                new TestPattern("F.T.TTe+").Matches(test).ShouldBe(false);
                new TestPattern("Cl.MDW").Matches(test).ShouldBe(false);
            }

            var testPattern = new TestPattern("C");
            testPattern.Matches(childClassChildMethod).ShouldBe(true);
            testPattern.Matches(parentClassParentMethod).ShouldBe(true);
            testPattern.Matches(childClassParentMethod).ShouldBe(true);

            testPattern = new TestPattern("CC");
            testPattern.Matches(childClassChildMethod).ShouldBe(true);
            testPattern.Matches(parentClassParentMethod).ShouldBe(false);
            testPattern.Matches(childClassParentMethod).ShouldBe(true);

            testPattern = new TestPattern("CC.MDW");
            testPattern.Matches(childClassChildMethod).ShouldBe(true);
            testPattern.Matches(parentClassParentMethod).ShouldBe(false);
            testPattern.Matches(childClassParentMethod).ShouldBe(true);

            testPattern = new TestPattern("C.MDW");
            testPattern.Matches(childClassChildMethod).ShouldBe(true);
            testPattern.Matches(parentClassParentMethod).ShouldBe(true);
            testPattern.Matches(childClassParentMethod).ShouldBe(true);

            testPattern = new TestPattern("C.MDWC");
            testPattern.Matches(childClassChildMethod).ShouldBe(true);
            testPattern.Matches(parentClassParentMethod).ShouldBe(false);
            testPattern.Matches(childClassParentMethod).ShouldBe(false);

            testPattern = new TestPattern("C.MDWP");
            testPattern.Matches(childClassChildMethod).ShouldBe(false);
            testPattern.Matches(parentClassParentMethod).ShouldBe(true);
            testPattern.Matches(childClassParentMethod).ShouldBe(true);
            
            testPattern = new TestPattern("ChildClass");
            testPattern.Matches(childClassChildMethod).ShouldBe(true);
            testPattern.Matches(parentClassParentMethod).ShouldBe(false);
            testPattern.Matches(childClassParentMethod).ShouldBe(true);

            testPattern = new TestPattern("ChildClass.MethodDefinedWithin");
            testPattern.Matches(childClassChildMethod).ShouldBe(true);
            testPattern.Matches(parentClassParentMethod).ShouldBe(false);
            testPattern.Matches(childClassParentMethod).ShouldBe(true);

            testPattern = new TestPattern("ChildClass.MethodDefinedWithinP");
            testPattern.Matches(childClassChildMethod).ShouldBe(false);
            testPattern.Matches(parentClassParentMethod).ShouldBe(false);
            testPattern.Matches(childClassParentMethod).ShouldBe(true);

            testPattern = new TestPattern("ChildClass.Met*odDefinedWithin");
            testPattern.Matches(childClassChildMethod).ShouldBe(true);
            testPattern.Matches(parentClassParentMethod).ShouldBe(false);
            testPattern.Matches(childClassParentMethod).ShouldBe(true);

            testPattern = new TestPattern("ChildClass.Met*odDefinedWithinP");
            testPattern.Matches(childClassChildMethod).ShouldBe(false);
            testPattern.Matches(parentClassParentMethod).ShouldBe(false);
            testPattern.Matches(childClassParentMethod).ShouldBe(true);

            testPattern = new TestPattern("*M*e*t*h*o*d*D*e*f*i*n*e*d*W*i*t*h*i*n*P");
            testPattern.Matches(childClassChildMethod).ShouldBe(false);
            testPattern.Matches(parentClassParentMethod).ShouldBe(true);
            testPattern.Matches(childClassParentMethod).ShouldBe(true);

            testPattern = new TestPattern("C*.M*D*W*P");
            testPattern.Matches(childClassChildMethod).ShouldBe(false);
            testPattern.Matches(parentClassParentMethod).ShouldBe(true);
            testPattern.Matches(childClassParentMethod).ShouldBe(true);
        }
    }
}