using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fixie.Samples.Parameterized
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .InTheSameNamespaceAs(typeof(CustomConvention))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid());

            ClassExecution
                .CreateInstancePerClass()
                .SortCases((caseA, caseB) => String.Compare(caseA.Name, caseB.Name, StringComparison.Ordinal));

            Parameters
                .Add<InputAttributeParameterSource>()
                .Add<TestCaseSourceAttributeParameterSource>();
        }

        class InputAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                return method.GetCustomAttributes<InputAttribute>(true).Select(input => input.Parameters);
            }
        }

        class TestCaseSourceAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                var testInvocations = new List<object[]>();

                var testCaseSourceAttributes = method.GetCustomAttributes<TestCaseSourceAttribute>(true).ToList();

                foreach (var attribute in testCaseSourceAttributes)
                {
                    var sourceType = attribute.SourceType ?? method.ReflectedType;

                    if (sourceType == null)
                        throw new Exception("Could not find source type for method " + method.Name);

                    var members = sourceType.GetMember(attribute.SourceName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                    if (members.Length != 1)
                        throw new Exception($"Found {members.Length} members of name {attribute.SourceName} in {sourceType}");

                    var member = members[0];

                    var field = member as FieldInfo;
                    if (field != null)
                    {
                        if (field.IsStatic)
                        {
                            var val = field.GetValue(null) as IEnumerable<object[]>;
                            testInvocations.AddRange(val);
                        }
                        else
                        {
                            throw new Exception($"Field {field.Name} must be static to be used as TestCaseSource input");
                        }
                    }
                
                    var property = member as PropertyInfo;
                    if (property != null)
                    {
                        if (property.GetGetMethod(true).IsStatic)
                        {
                            var val = property.GetValue(null, null) as IEnumerable<object[]>;
                            testInvocations.AddRange(val);
                        }
                        else
                        {
                            throw new Exception($"Property {property.Name} must have a static get() method to be used as TestCaseSource input");
                        }
                    }

                    var m = member as MethodInfo;
                    if (m != null)
                    {
                        if (m.IsStatic)
                        {
                            var val = m.Invoke(null, null) as IEnumerable<object[]>;
                            testInvocations.AddRange(val);
                        }
                        else
                        {
                            throw new Exception($"Method {m.Name} must be static to be used as TestCaseSource input");
                        }
                    }
                }

                return testInvocations;
            }
        }
    }
}