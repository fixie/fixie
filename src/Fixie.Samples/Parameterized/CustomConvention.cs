using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie.Samples.Parameterized
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .Where(type => type.IsInNamespace(GetType().Namespace))
                .NameEndsWith("Tests");

            Methods
                .Where(method => method.IsVoid());

            ClassExecution
                .CreateInstancePerTestClass();

            Parameters(FromInputAttributes);
        }
        
        IEnumerable<object[]> FromInputAttributes(MethodInfo method)
        {
            var inputAttributes = method.GetCustomAttributes<InputAttribute>(true).ToArray();

            if (!inputAttributes.Any())
            {
                yield return null;
            }
            else
            {
                foreach (var input in inputAttributes)
                    yield return input.Parameters;
            }
        }
        
        object Default(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}