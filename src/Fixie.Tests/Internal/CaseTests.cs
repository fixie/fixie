namespace Fixie.Tests.Internal
{
    using System.IO;
    using System.Linq;
    using Assertions;
    using Fixie.Internal;
    using Fixie.Reports;

    public class CaseTests
    {
        public void ShouldHaveMethodInfoIncludingResolvedGenericArguments()
        {
            var method = Case("Returns").ResolvedMethod;
            method.Name.ShouldBe("Returns");
            method.GetParameters().ShouldBeEmpty();

            method = Case("Parameterized", 123, true, 'a', "s", null, this).ResolvedMethod;
            method.Name.ShouldBe("Parameterized");
            method.GetParameters()
                .Select(x => x.ParameterType)
                .ShouldBe(
                    typeof(int), typeof(bool),
                    typeof(char), typeof(string),
                    typeof(string), typeof(object),
                    typeof(CaseTests));

            method = Case("Generic", 123, true, "a", "b").ResolvedMethod;
            method.Name.ShouldBe("Generic");
            method.GetParameters()
                .Select(x => x.ParameterType)
                .ShouldBe(typeof(int), typeof(bool), typeof(string), typeof(string));

            method = Case("Generic", 123, true, 1, null).ResolvedMethod;
            method.Name.ShouldBe("Generic");
            method.GetParameters()
                .Select(x => x.ParameterType)
                .ShouldBe(typeof(int), typeof(bool), typeof(int), typeof(int));

            method = Case("Generic", 123, 1.23m, "a", null).ResolvedMethod;
            method.Name.ShouldBe("Generic");
            method.GetParameters()
                .Select(x => x.ParameterType)
                .ShouldBe(typeof(int), typeof(decimal), typeof(string), typeof(string));

            method = Case("ConstrainedGeneric", 1).ResolvedMethod;
            method.Name.ShouldBe("ConstrainedGeneric");
            method.GetParameters().Single().ParameterType.ShouldBe(typeof(int));

            method = Case("ConstrainedGeneric", true).ResolvedMethod;
            method.Name.ShouldBe("ConstrainedGeneric");
            method.GetParameters().Single().ParameterType.ShouldBe(typeof(bool));
            var resolvedParameterType = method.GetParameters().Single().ParameterType;
            resolvedParameterType.Name.ShouldBe("Boolean");
            resolvedParameterType.IsGenericParameter.ShouldBe(false);

            method = Case("ConstrainedGeneric", "Incompatible").ResolvedMethod;
            method.Name.ShouldBe("ConstrainedGeneric");
            var unresolvedParameterType = method.GetParameters().Single().ParameterType;
            unresolvedParameterType.Name.ShouldBe("T");
            unresolvedParameterType.IsGenericParameter.ShouldBe(true);
        }

        static Case Case(string methodName, params object?[] parameters)
            => Case<CaseTests>(methodName, parameters);

        static Case Case<TTestClass>(string methodName, params object?[] parameters)
        {
            var caseMethod = typeof(TTestClass).GetInstanceMethod(methodName);

            var console = TextWriter.Null;
            var recordNothing = new ExecutionRecorder(new RecordingWriter(console), new Bus(console, new Report[] { }));
            var test = new Test(recordNothing, caseMethod);

            return new Case(test.Method, parameters);
        }

        void Returns()
        {
        }

        void Parameterized(int i, bool b, char ch, string s1, string s2, object obj, CaseTests complex)
        {
        }

        void Generic<T1, T2>(int i, T1 t1, T2 t2a, T2 t2b)
        {
        }

        void ConstrainedGeneric<T>(T t) where T : struct
        {
        }
    }
}
