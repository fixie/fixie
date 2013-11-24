using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;
using Should;

namespace Fixie.Tests.TestMethods
{
    public class ParameterizedMethodTests
    {
        public void ShouldAllowConventionToGeneratePotentiallyManySetsOfInputParametersPerMethod()
        {
            var listener = new StubListener();

            var convention = new SelfTestConvention();
            convention.Parameters(ParametersFromAttributesWithTypeDefaultFallback);

            convention.Execute(listener, typeof(ParameterizedTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.IntArg(0) passed.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes(1, 1, 2) passed.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes(1, 2, 3) passed.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes(5, 5, 11) failed: Expected sum of 11 but was 10.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.ZeroArgs passed.");
        }

        public void ShouldFailWithClearExplanationWhenInputParameterGenerationHasNotBeenCustomizedYetTestMethodAcceptsParameters()
        {
            var listener = new StubListener();

            var convention = new SelfTestConvention();

            convention.Execute(listener, typeof(ParameterizedTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.IntArg failed: This parameterized test could not be executed, because no input values were available.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes failed: This parameterized test could not be executed, because no input values were available.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.ZeroArgs passed.");
        }

        public void ShouldFailWithClearExplanationWhenInputParameterGenerationHasBeenCustomizedYetYieldsZeroSetsOfInputs()
        {
            var listener = new StubListener();

            var convention = new SelfTestConvention();
            convention.Parameters(ZeroSetsOfInputParameters);

            convention.Execute(listener, typeof(ParameterizedTestClass));

            listener.Entries.ShouldEqual(
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.IntArg failed: This parameterized test could not be executed, because no input values were available.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.MultipleCasesFromAttributes failed: This parameterized test could not be executed, because no input values were available.",
                "Fixie.Tests.TestMethods.ParameterizedMethodTests+ParameterizedTestClass.ZeroArgs passed.");
        }

        public void ShouldResolveGenericTypeParameters()
        {
            var listener = new StubListener();
            
            var convention = new SelfTestConvention();
            convention.Parameters(ParametersFromAttributesWithTypeDefaultFallback);
            
            convention.Execute(listener, typeof(GenericTestClass));
            const string cln = "Fixie.Tests.TestMethods.ParameterizedMethodTests+GenericTestClass.";
            listener.Entries.ShouldContain(cln+"MultipleGenericArgumentsMultipleParameters<System.Int32, System.Object>(123, null, 456, System.Int32, System.Object) passed.");
            listener.Entries.ShouldContain(cln+"MultipleGenericArgumentsMultipleParameters<System.Int32, System.String>(123, \"stringArg1\", 456, System.Int32, System.String) passed.");
            listener.Entries.ShouldContain(cln+"MultipleGenericArgumentsMultipleParameters<System.String, System.Object>(\"stringArg\", null, null, System.String, System.Object) passed.");                                
            listener.Entries.ShouldContain(cln+"MultipleGenericArgumentsMultipleParameters<System.String, System.Object>(\"stringArg1\", null, \"stringArg2\", System.String, System.Object) passed.");                
            listener.Entries.ShouldContain(cln+"MultipleGenericArgumentsMultipleParameters<System.String, System.String>(null, \"stringArg1\", \"stringArg2\", System.String, System.String) passed.");                
            listener.Entries.ShouldContain(cln+"SingleGenericArgument<System.String>(\"stringArg\", System.String) passed.");
            listener.Entries.ShouldContain(cln+"SingleGenericArgument<System.Int32>(123, System.Int32) passed.");
            listener.Entries.ShouldContain(cln+"SingleGenericArgument<System.Object>(null, System.Object) passed.");
            listener.Entries.ShouldContain(cln+"SingleGenericArgumentMultipleParameters<System.Object>(\"stringArg\", 123, System.Object) passed.");
            listener.Entries.ShouldContain(cln+"SingleGenericArgumentMultipleParameters<System.String>(\"stringArg\", null, System.String) passed.");
            listener.Entries.ShouldContain(cln+"SingleGenericArgumentMultipleParameters<System.String>(\"stringArg1\", \"stringArg2\", System.String) passed.");
            listener.Entries.ShouldContain(cln+"SingleGenericArgumentMultipleParameters<System.Object>(123, \"stringArg\", System.Object) passed.");
            listener.Entries.ShouldContain(cln+"SingleGenericArgumentMultipleParameters<System.Int32>(123, 456, System.Int32) passed.");
            listener.Entries.ShouldContain(cln+"SingleGenericArgumentMultipleParameters<System.Object>(123, null, System.Object) passed.");
            listener.Entries.ShouldContain(cln+"SingleGenericArgumentMultipleParameters<System.String>(null, \"stringArg\", System.String) passed.");
            listener.Entries.ShouldContain(cln+"SingleGenericArgumentMultipleParameters<System.Object>(null, null, System.Object) passed.");
        }

        IEnumerable<object[]> ParametersFromAttributesWithTypeDefaultFallback(MethodInfo method)
        {
            var parameters = method.GetParameters();

            var inputAttributes = method.GetCustomAttributes<InputAttribute>(true).ToArray();

            if (inputAttributes.Any())
            {
                foreach (var input in inputAttributes)
                    yield return input.Parameters;
            }
            else
            {
                yield return parameters.Select(p => Default(p.ParameterType)).ToArray();
            }
        }

        IEnumerable<object[]> ZeroSetsOfInputParameters(MethodInfo method)
        {
            yield break;
        }

        object Default(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        class ParameterizedTestClass
        {
            public void ZeroArgs()
            {
            }

            public void IntArg(int i)
            {
                if (i != 0)
                    throw new Exception("Expected 0, but was " + i);
            }

            [Input(1, 1, 2)]
            [Input(1, 2, 3)]
            [Input(5, 5, 11)]
            public void MultipleCasesFromAttributes(int a, int b, int expectedSum)
            {
                if (a + b != expectedSum)
                    throw new Exception(string.Format("Expected sum of {0} but was {1}.", expectedSum, a + b));
            }

        }

        class GenericTestClass
        {
           [Input(null, "stringArg1", "stringArg2", typeof(string), typeof(string))]           
           [Input("stringArg", null, null, typeof(string), typeof(object))]
           [Input(123, null, 456, typeof(int), typeof(object))]
           [Input("stringArg1", null, "stringArg2", typeof(string), typeof(object))]
           [Input(123, "stringArg1", 456, typeof(int), typeof(string))]           
           public void MultipleGenericArgumentsMultipleParameters<T1, T2>(T1 genArg1a, T2 genArg2, T1 genArg1b, Type t1, Type t2)
           {
               typeof(T1).ShouldEqual(t1, string.Format("Expected {0}+{1} to resolve to type {2} but found type {3}", Fmt(genArg1a), Fmt(genArg1b), t1, typeof(T1)));
               typeof(T2).ShouldEqual(t2, string.Format("Expected {0} to resolve to type {1} but found type {2}", Fmt(genArg2), t2, typeof(T2)));
           }

           [Input(123, 456, typeof(int))]
           [Input(123, null, typeof(object))]
           [Input(null, null, typeof(object))]
           [Input("stringArg1", "stringArg2", typeof(string))]
           [Input(123, "stringArg", typeof(object))]
           [Input("stringArg", 123, typeof(object))]
           [Input(null, "stringArg", typeof(string))]
           [Input("stringArg", null, typeof(string))]
           public void SingleGenericArgumentMultipleParameters<T>(T genArg1, T genArg2, Type t)
           {
              typeof(T).ShouldEqual(t, string.Format("Expected {0}+{1} to resolve to type {2} but found type {3}", Fmt(genArg1), Fmt(genArg2), t, typeof(T)));
           }
         
           [Input(123, typeof(int))]
           [Input("stringArg", typeof(string))]
           [Input(null, typeof(object))]
           public void SingleGenericArgument<T>(T genArg, Type t)
           {
              typeof(T).ShouldEqual(t, string.Format("Expected {0} to resolve to type {1} but found type {2}", Fmt(genArg), t, typeof(T)));
           }

           private static string Fmt(object obj)
           {
              if (obj == null)
                 return "[null]";
              return obj.ToString();
           }

        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class InputAttribute : Attribute
        {
            public InputAttribute(params object[] parameters)
            {
                Parameters = parameters;
            }

            public object[] Parameters { get; private set; }
        }
    }

 
}