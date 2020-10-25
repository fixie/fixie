namespace Fixie
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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

            if (returnType == typeof(void))
            {
                if (method.HasAsyncKeyword())
                    throw new NotSupportedException(
                        "`async void` test methods are not supported. Declare " +
                        "the test method as `async Task` to ensure the task " +
                        "actually runs to completion.");
            }
            else
            {
                var isFSharpAsync = IsFSharpAsync(returnType);
                if (returnType != typeof(Task) &&
                    returnType != typeof(ValueTask) &&
                    !isFSharpAsync)
                {
                    if (returnType.IsGenericType)
                    {
                        var genericTypeDefinition = returnType.GetGenericTypeDefinition();

                        if (genericTypeDefinition == typeof(Task<>))
                        {
                            if (method.HasAsyncKeyword())
                            {
                                throw new NotSupportedException(
                                    "`async Task<T>` test methods are not supported. Declare " +
                                    "the test method as `async Task` to ensure the task " +
                                    "actually runs to completion.");
                            }

                            throw new NotSupportedException(
                                "`Task<T>` test methods are not supported. Declare " +
                                "the test method as `Task` to ensure the task " +
                                "actually runs to completion.");
                        }

                        if (genericTypeDefinition == typeof(ValueTask<>))
                        {
                            throw new NotSupportedException(
                                "`async ValueTask<T>` test methods are not supported. Declare " +
                                "the test method as `async ValueTask` to ensure the task " +
                                "actually runs to completion.");
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
                if (returnType == typeof(void))
                    return;
                
                throw new NullReferenceException(
                    "This asynchronous test returned null, but " +
                    "a non-null awaitable object was expected.");
            }

            if (!ConvertibleToTask(result, out var task))
                return; //This is severe now. When I'm done it should be impossible to get here because I will have essentially allow-listed only signatures that would be satisfied by ConvertibleToTask! I should straight up throw because it would mean we definitely have a bug!

            if (task.Status == TaskStatus.Created)
                throw new InvalidOperationException("The test returned a non-started task, which cannot be awaited. Consider using Task.Run or Task.Factory.StartNew.");

            await task;
        }

        static bool HasAsyncKeyword(this MethodInfo method)
        {
            return method.Has<AsyncStateMachineAttribute>();
        }

        static bool ConvertibleToTask(object result, [NotNullWhen(true)] out Task? task)
        {
            if (result is Task t)
            {
                task = t;
                return true;
            }

            if (result is ValueTask vt)
            {
                task = vt.AsTask();
                return true;
            }

            var resultType = result.GetType();

            if (IsFSharpAsync(resultType))
            {
                task = ConvertFSharpAsyncToTask(result, resultType);
                return true;
            }

            task = null;
            return false;
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
