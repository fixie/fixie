using System;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Samples.ChangeCaseName
{
    public class CustomCaseBuilder : ICaseBuilder
    {
        public Case Build(Type testClass, MethodInfo caseMethod, object[] parameters)
        {
            return new CustomCase(testClass, caseMethod, parameters);
        }

        public class CustomCase : Case
        {
            public CustomCase(Type testClass, MethodInfo caseMethod, params object[] parameters):
                base(testClass, caseMethod, parameters)
            {

            }

            /// <summary>
            /// Try naively to remove "tests" or "test" in the case's name
            /// </summary>
            /// <returns></returns>
            protected override string GetName()
            {
                return base.GetName().Replace("Tests", "").Replace("Test", "");
            }
        }

    }
}
