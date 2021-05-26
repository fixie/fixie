namespace Fixie
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Internal;

    public static class MethodInfoExtensions
    {
        static MethodInfo? startAsTask;

        public static async Task<object?> CallAsync(this MethodInfo method, object? instance, params object?[] parameters)
        {
            var resolvedMethod = method.TryResolveTypeArguments(parameters);

            var returnType = resolvedMethod.ReturnType;

            var isVoid = returnType == typeof(void);

            if (isVoid)
            {
                if (resolvedMethod.HasAsyncKeyword())
                    throw new NotSupportedException(
                        "`async void` methods are not supported. Declare " +
                        "the method as `async Task` to ensure the task " +
                        "actually runs to completion.");
            }

            if (resolvedMethod.ContainsGenericParameters)
                throw new Exception("Could not resolve type parameters for generic method.");

            object? result;

            try
            {
                result = resolvedMethod.Invoke(instance, parameters.Length == 0 ? null : parameters);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception);
            }

            if (isVoid)
                return null;

            if (IsAwaitable(returnType, result, out var task, out var taskHasResult))
            {
                if (task.Status == TaskStatus.Created)
                    throw new InvalidOperationException(
                        "The method returned a non-started task, which cannot be awaited. " +
                        "Consider using Task.Run or Task.Factory.StartNew.");

                await task;

                return taskHasResult
                    ? task.GetType().GetProperty("Result")!.GetValue(task)
                    : null;
            }

            return result;
        }

        static bool IsAwaitable(Type returnType, object? result, [NotNullWhen(true)] out Task? task, out bool taskHasResult)
        {
            if (returnType.IsGenericType)
            {
                var genericDefinition = returnType.GetGenericTypeDefinition();

                if (genericDefinition == typeof(ValueTask<>))
                {
                    task = (Task)typeof(ValueTask<>)
                        .MakeGenericType(returnType.GetGenericArguments().Single())
                        .GetMethod("AsTask")!.Invoke(result, null)!;
                    taskHasResult = true;
                    return true;
                }

                if (genericDefinition == typeof(Task<>))
                {
                    if (result == null)
                        ThrowForNullAwaitable();
                    task = (Task)result;
                    taskHasResult = true;
                    return true;
                }

                if (genericDefinition.FullName == "Microsoft.FSharp.Control.FSharpAsync`1")
                {
                    if (result == null)
                        ThrowForNullAwaitable();
                    task = ConvertFSharpAsyncToTask(result, returnType);
                    taskHasResult = true;
                    return true;
                }
            }
            else if (returnType == typeof(Task))
            {
                if (result == null)
                    ThrowForNullAwaitable();
                task = (Task)result;
                taskHasResult = false;
                return true;
            }
            else if (returnType == typeof(ValueTask))
            {
                task = ((ValueTask)result!).AsTask();
                taskHasResult = false;
                return true;
            }
            
            task = null;
            taskHasResult = false;
            return false;
        }

        [DoesNotReturn]
        static void ThrowForNullAwaitable()
        {
            throw new NullReferenceException(
                "This asynchronous method returned null, but " +
                "a non-null awaitable object was expected.");
        }

        internal static async Task RunTestMethodAsync(this MethodInfo method, object? instance, params object?[] parameters)
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

            if (isVoid)
                return;

            if (result == null)
                throw new NullReferenceException(
                    "This asynchronous test returned null, but " +
                    "a non-null awaitable object was expected.");

            if (ConvertToTask(result, out var task))
            {
                if (task.Status == TaskStatus.Created)
                    throw new InvalidOperationException(
                        "The test returned a non-started task, which cannot be awaited. " +
                        "Consider using Task.Run or Task.Factory.StartNew.");

                await task;
            }
            else
            {
                var resultType = result.GetType();
            
                if (IsFSharpAsync(resultType))
                {
                    task = ConvertFSharpAsyncToTask(result, resultType);
                    await task;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"The test returned an object with an unsupported type: {resultType.FullName}");
                }
            }
        }

        static bool HasAsyncKeyword(this MethodInfo method)
        {
            return method.Has<AsyncStateMachineAttribute>();
        }

        static bool ConvertToTask(object result, [NotNullWhen(true)] out Task? task)
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

            try
            {
                return (Task) genericStartAsTask.Invoke(null, new[] { result, null, null })!;
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception);
            }
        }
    }
}
