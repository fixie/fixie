namespace Fixie.Samples.MbUnitStyle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .HasOrInherits<TestFixture>();

            Methods
                .HasOrInherits<Test>()
                .OrderBy(x => x.Name, StringComparer.Ordinal);

            ClassExecution
                .Lifecycle<SetUpTearDown>();

            Parameters
                .Add<RowAttributeParameterSource>()
                .Add<ColumnAttributeParameterSource>();
        }

        class RowAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                ParameterInfo[] parameterInfos = method.GetParameters();

                var rowAttributes = method.GetCustomAttributes<RowAttribute>(true);

                foreach (var rowAttribute in rowAttributes)
                {
                    object[] parameters = rowAttribute.Parameters;

                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        parameters[i] = ChangeType(parameters[i], parameterInfos[i].ParameterType);
                    }

                    yield return parameters;
                }
            }
        }

        class ColumnAttributeParameterSource : ParameterSource
        {
            public IEnumerable<object[]> GetParameters(MethodInfo method)
            {
                return CartesianProduct(Columns(method));
            }

            static IEnumerable<object[]> Columns(MethodInfo method)
            {
                ParameterInfo[] parameterInfos = method.GetParameters();

                if (parameterInfos.Length == 0)
                    return null;

                if (parameterInfos[0].GetCustomAttributes<ColumnAttribute>(true).Any() == false)
                    return null;

                return GetColumnParameters(parameterInfos);
            }

            static IEnumerable<object[]> GetColumnParameters(ParameterInfo[] parameterInfos)
            {
                foreach (var parameterInfo in parameterInfos)
                {
                    yield return parameterInfo
                        .GetCustomAttributes<ColumnAttribute>(true)
                        .Single()
                        .Parameters
                        .Select(x => ChangeType(x, parameterInfo.ParameterType))
                        .ToArray();
                }
            }

            static IEnumerable<object[]> CartesianProduct(IEnumerable<object[]> sequences)
            {
                if (sequences == null)
                    return new object[][] { };

                //See http://blogs.msdn.com/b/ericlippert/archive/2010/06/28/computing-a-cartesian-product-with-linq.aspx

                IEnumerable<object[]> emptyProduct = new[] { new object[] { } };
                
                return sequences.Aggregate(
                    emptyProduct,
                    (accumulator, sequence) =>
                        from accseq in accumulator
                        from item in sequence
                        select accseq.Concat(new[] { item }).ToArray());
            }
        }

        static object ChangeType(object parameter, Type type)
        {
            if (parameter != null && parameter.GetType() != type)
            {
                try
                {
                    parameter = Convert.ChangeType(parameter, type);
                }
                catch { }
            }

            return parameter;
        }
    }

    class SetUpTearDown : Lifecycle
    {
        public void Execute(RunContext runContext, Action<CaseAction> runCases)
        {
            var instance = Activator.CreateInstance(runContext.TestClass);

            runContext.Execute<FixtureSetUp>(instance);
            runCases(@case =>
            {
                runContext.Execute<SetUp>(instance);

                @case.Execute(instance);

                HandleExpectedExceptions(@case);

                runContext.Execute<TearDown>(instance);
            });
            runContext.Execute<FixtureTearDown>(instance);

            (instance as IDisposable)?.Dispose();
        }

        static void HandleExpectedExceptions(Case @case)
        {
            var attribute = @case.Method.GetCustomAttributes<ExpectedExceptionAttribute>(false).SingleOrDefault();

            if (attribute == null)
                return;

            var exception = @case.Exception;

            try
            {
                if (exception == null)
                    throw new Exception("Expected exception of type " + attribute.ExpectedException + ".");

                if (!attribute.ExpectedException.IsAssignableFrom(exception.GetType()))
                {
                    throw new Exception(
                        "Expected exception of type " + attribute.ExpectedException + " but an exception of type " +
                        exception.GetType() + " was thrown.", exception);
                }

                if (attribute.ExpectedMessage != null && exception.Message != attribute.ExpectedMessage)
                {
                    throw new Exception(
                        "Expected exception message '" + attribute.ExpectedMessage + "'" + " but was '" + exception.Message + "'.",
                        exception);
                }

                @case.Pass();
            }
            catch (Exception failureReason)
            {
                @case.Fail(failureReason);
            }
        }
    }
}
