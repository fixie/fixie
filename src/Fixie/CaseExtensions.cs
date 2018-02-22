namespace Fixie
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public static class CaseExtensions
    {
        public static object Execute(this Case @case, object instance)
        {
            try
            {
                var result = @case.Method.Execute(instance, @case.Parameters);
                @case.Pass();
                return result;
            }
            catch (Exception exception)
            {
                @case.Fail(exception);
                return null;
            }
        }

        static object Execute(this MethodInfo method, object instance, object[] parameters)
        {
            bool isDeclaredAsync = method.IsAsync();

            if (isDeclaredAsync && method.IsVoid())
                throw new NotSupportedException(
                    "Async void methods are not supported. Declare async methods with a " +
                    "return type of Task to ensure the task actually runs to completion.");

            if (method.ContainsGenericParameters)
                throw new Exception("Could not resolve type parameters for generic method.");

            object result;

            try
            {
                result = method.Invoke(instance, parameters);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception.InnerException);
            }

            if (isDeclaredAsync)
            {
                var task = (Task) result;
                try
                {
                    task.Wait();
                }
                catch (AggregateException exception)
                {
                    throw new PreservedException(exception.InnerExceptions.First());
                }

                if (method.ReturnType.IsGenericType)
                {
                    var property = task.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);

                    result = property.GetValue(task, null);
                }
                else
                {
                    result = null;
                }
            }

            return result;
        }
    }
}