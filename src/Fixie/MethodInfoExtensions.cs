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
            => await method.TryResolveTypeArguments(parameters).CallResolvedMethodAsync(instance, parameters);

        internal static async Task<object?> CallResolvedMethodAsync(this MethodInfo resolvedMethod, object? instance, object?[] parameters)
        {
            var isVoid = resolvedMethod.ReturnType == typeof(void);

            if (isVoid && resolvedMethod.HasAsyncKeyword())
                ThrowForAsyncVoid(resolvedMethod);

            if (resolvedMethod.ContainsGenericParameters)
                ThrowForUnresolvedTypeParameters(resolvedMethod);

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

            if (IsAwaitable(resolvedMethod, result, out var task, out var taskHasResult))
            {
                if (task.Status == TaskStatus.Created)
                    ThrowForNonStartedTask(resolvedMethod);

                await task;

                return taskHasResult
                    ? task.GetType().GetProperty("Result")!.GetValue(task)
                    : null;
            }

            return result;
        }

        static bool IsAwaitable(MethodInfo resolvedMethod, object? result, [NotNullWhen(true)] out Task? task, out bool taskHasResult)
        {
            var returnType = resolvedMethod.ReturnType;

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
                        ThrowForNullAwaitable(resolvedMethod);
                    task = (Task)result;
                    taskHasResult = true;
                    return true;
                }

                if (genericDefinition.FullName == "Microsoft.FSharp.Control.FSharpAsync`1")
                {
                    if (result == null)
                        ThrowForNullAwaitable(resolvedMethod);
                    task = ConvertFSharpAsyncToTask(result, returnType);
                    taskHasResult = true;
                    return true;
                }

                if (genericDefinition == typeof(IAsyncEnumerable<>) ||
                    genericDefinition == typeof(IAsyncEnumerator<>))
                    ThrowForUnsupportedAwaitable(resolvedMethod);
            }
            else if (returnType == typeof(Task))
            {
                if (result == null)
                    ThrowForNullAwaitable(resolvedMethod);
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
            
            if (returnType.Has<AsyncMethodBuilderAttribute>())
                ThrowForUnsupportedAwaitable(resolvedMethod);

            task = null;
            taskHasResult = false;
            return false;
        }

        [DoesNotReturn]
        static void ThrowForAsyncVoid(MethodInfo resolvedMethod)
            => throw new NotSupportedException(
                $"The method {resolvedMethod.Name} is declared as `async void`, " +
                "which is not supported. To ensure the reliability of the test " +
                "runner, declare the method as `async Task`.");

        [DoesNotReturn]
        static void ThrowForUnresolvedTypeParameters(MethodInfo resolvedMethod)
            => throw new InvalidOperationException(
                $"The type parameters for generic method {resolvedMethod.Name} " +
                "could not be resolved.");

        [DoesNotReturn]
        static void ThrowForNullAwaitable(MethodInfo method)
            => throw new NullReferenceException(
                $"The asynchronous method {method.Name} returned null, but " +
                "a non-null awaitable object was expected.");

        [DoesNotReturn]
        static void ThrowForNonStartedTask(MethodInfo resolvedMethod)
            => throw new InvalidOperationException(
                $"The method {resolvedMethod.Name} returned a non-started task, which " +
                "cannot be awaited. Consider using Task.Run or Task.Factory.StartNew.");

        [DoesNotReturn]
        static void ThrowForUnsupportedAwaitable(MethodInfo method)
            => throw new NotSupportedException(
                $"The return type of method {method.Name} is an unsupported awaitable type. " +
                "To ensure the reliability of the test runner, declare " +
                "the method return type as `Task`, `Task<T>`, `ValueTask`, " +
                "or `ValueTask<T>`.");

        static bool HasAsyncKeyword(this MethodInfo method)
        {
            return method.Has<AsyncStateMachineAttribute>();
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
