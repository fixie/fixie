namespace Fixie
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
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
                ExceptionDispatchInfo.Capture(exception.InnerException!).Throw();
                throw; // Unreachable.
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
                    task = (Task)returnType.GetMethod("AsTask")!.Invoke(result, null)!;
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

                if (genericDefinition == typeof(IAsyncEnumerable<>) ||
                    genericDefinition == typeof(IAsyncEnumerator<>))
                {
                    ThrowForUnsupportedAwaitable();
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
            else if (returnType.Has<AsyncMethodBuilderAttribute>())
            {
                ThrowForUnsupportedAwaitable();
            }
            
            task = null;
            taskHasResult = false;
            return false;
        }

        internal static async Task RunTestMethodAsync(this MethodInfo resolvedMethod, object? instance, object?[] parameters)
        {
            var returnType = resolvedMethod.ReturnType;

            var isVoid = returnType == typeof(void);

            if (isVoid)
            {
                if (resolvedMethod.HasAsyncKeyword())
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
                    throw new NotSupportedException(
                        "Test method return type is not supported. Declare " +
                        "the test method return type as `void`, `Task`, or " +
                        "`ValueTask`.");
                }
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
                ExceptionDispatchInfo.Capture(exception.InnerException!).Throw();
                throw; // Unreachable.
            }

            if (isVoid)
                return;

            if (IsAwaitable(returnType, out var task, result))
            {
                if (task.Status == TaskStatus.Created)
                    throw new InvalidOperationException(
                        "The test returned a non-started task, which cannot be awaited. " +
                        "Consider using Task.Run or Task.Factory.StartNew.");

                await task;
            }
            else
            {
                throw new InvalidOperationException(
                    $"The test returned an object with an unsupported type: {returnType.FullName}");
            }
        }

        [DoesNotReturn]
        static void ThrowForNullAwaitable()
        {
            throw new NullReferenceException(
                "This asynchronous method returned null, but " +
                "a non-null awaitable object was expected.");
        }

        static void ThrowForUnsupportedAwaitable()
        {
            throw new NotSupportedException(
                "The method return type is an unsupported awaitable type. " +
                "To ensure the reliability of the test runner, declare " +
                "the method return type as `Task`, `Task<T>`, `ValueTask`, " +
                "or `ValueTask<T>`.");
        }

        static bool HasAsyncKeyword(this MethodInfo method)
        {
            return method.Has<AsyncStateMachineAttribute>();
        }

        static bool IsAwaitable(Type returnType, [NotNullWhen(true)] out Task? task, object? result)
        {
            if (returnType == typeof(Task))
            {
                if (result == null)
                    ThrowForNullAwaitable();
                
                task = (Task)result;
                return true;
            }
            else if (returnType == typeof(ValueTask))
            {
                task = ((ValueTask)result!).AsTask();
                return true;
            }
            else if (IsFSharpAsync(returnType))
            {
                if (result == null)
                    ThrowForNullAwaitable();

                task = ConvertFSharpAsyncToTask(result, returnType);
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
                ExceptionDispatchInfo.Capture(exception.InnerException!).Throw();
                throw; // Unreachable.
            }
        }
    }
}
