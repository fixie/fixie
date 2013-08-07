using System;
using Fixie.Conventions;

namespace Fixie.Samples.Inclusive
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            //In this example, the description of test classes is so inclusive that this convention
            //class itself could be mistaken for a test class. Since convention classes are
            //automatically excluded, though, ShouldNotBeCalled() will not be called.

            Classes
                .Where(type => type.IsInNamespace(GetType().Namespace));

            Cases = new MethodFilter()
                .Where(method => method.Void())
                .ZeroParameters();
        }

        public void ShouldNotBeCalled()
        {
            throw new Exception("This method should not be treated as a test.");
        }
    }
}