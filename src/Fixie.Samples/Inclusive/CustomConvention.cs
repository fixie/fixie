namespace Fixie.Samples.Inclusive
{
    using System;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            //In this example, the description of test classes is so inclusive that this convention
            //class itself could be mistaken for a test class. Since convention classes are
            //automatically excluded, though, ShouldNotBeCalled() will not be called.

            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention));
        }

        public void ShouldNotBeCalled()
        {
            throw new Exception("This method should not be treated as a test.");
        }
    }
}