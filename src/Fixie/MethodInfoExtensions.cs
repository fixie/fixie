namespace Fixie
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    static class MethodInfoExtensions
    {
        static MethodInfo? startAsTask;

        public static async Task ExecuteAsync(this MethodInfo method, object? instance, params object?[] parameters)
        {
            var returnType = method.ReturnType;

            var isVoid = returnType == typeof(void);

            if (isVoid)
            {
                if (method.HasAsyncKeyword())
                    throw new NotSupportedException(
                        "`async void` test methods are not supported. Declare " +
                        "the test method as `async Task` to ensure the task " +
                        "actually runs to completion.");
            }
            else
            {
                if (returnType != typeof(Task) &&
                    returnType != typeof(ValueTask) &&
                    !IsFSharpAsync(returnType))
                {
                    if (returnType.IsGenericType)
                    {
                        var genericTypeDefinition = returnType.GetGenericTypeDefinition();

                        if (genericTypeDefinition == typeof(Task<>))
                        {
                            var asyncPrefix = method.HasAsyncKeyword() ? "async " : "";

                            throw new NotSupportedException(
                                $"`{asyncPrefix}Task<T>` test methods are not supported. Declare " +
                                $"the test method as `{asyncPrefix}Task` to acknowledge that the " +
                                "`Result` will not be witnessed.");
                        }

                        if (genericTypeDefinition == typeof(ValueTask<>))
                        {
                            throw new NotSupportedException(
                                "`async ValueTask<T>` test methods are not supported. Declare " +
                                "the test method as `async ValueTask` to acknowledge that the " +
                                "`Result` will not be witnessed.");
                        }
                    }

                    throw new NotSupportedException(
                        "Test method return type is not supported. Declare " +
                        "the test method return type as `void`, `Task`, or `ValueTask`.");
                }
            }

            if (method.ContainsGenericParameters)
                throw new Exception("Could not resolve type parameters for generic method.");

            object? result;

            try
            {
                result = method.Invoke(instance, parameters.Length == 0 ? null : parameters);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception);
            }

            if (result == null)
            {
                if (isVoid)
                    return;
                
                throw new NullReferenceException(
                    "This asynchronous test returned null, but " +
                    "a non-null awaitable object was expected.");
            }

            var task = ConvertToTask(result);

            if (task.Status == TaskStatus.Created)
                throw new InvalidOperationException(
                    "The test returned a non-started task, which cannot be awaited. " +
                    "Consider using Task.Run or Task.Factory.StartNew.");

            await task;
        }

        static bool HasAsyncKeyword(this MethodInfo method)
        {
            return method.Has<AsyncStateMachineAttribute>();
        }

        static Task ConvertToTask(object result)
        {
            if (result is Task t)
                return t;

            if (result is ValueTask vt)
                return vt.AsTask();

            var resultType = result.GetType();
            
            if (IsFSharpAsync(resultType))
                return ConvertFSharpAsyncToTask(result, resultType);

            throw new InvalidOperationException(
                $"The test returned an object with an unsupported type: {resultType.FullName}");
        }

        static bool IsFSharpAsync(Type resultType)
        {
            return resultType.IsGenericType &&
                   resultType.GetGenericTypeDefinition().FullName == "Microsoft.FSharp.Control.FSharpAsync`1";
        }

        static Task ConvertFSharpAsyncToTask(object result, Type resultType)
        {
            try
            {
                if (startAsTask == null)
                    startAsTask = resultType
                        .Assembly
                        .GetType("Microsoft.FSharp.Control.FSharpAsync")!
                        .GetRuntimeMethods()
                        .Single(x => x.Name == "StartAsTask");
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Unable to locate F# Control.Async.StartAsTask method.", exception);
            }

            var genericStartAsTask = startAsTask.MakeGenericMethod(resultType.GetGenericArguments());

            return (Task) genericStartAsTask.Invoke(null, new[] { result, null, null })!;
        }
    }
}
