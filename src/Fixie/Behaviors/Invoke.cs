﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fixie.Behaviors
{
    public class Invoke : CaseBehavior
    {
        public void Execute(Case @case, object instance)
        {
            try
            {
                var member = @case.Member;

                var method = member as MethodInfo;
                var field = member as FieldInfo;
                object result;

                if (method != null)
                {
                    bool isDeclaredAsync = method.Async();

                    if (isDeclaredAsync && method.Void())
                        ThrowForUnsupportedAsyncVoid();

                    try
                    {
                        result = method.Invoke(instance, null);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw new PreservedException(ex.InnerException);
                    }

                    if (isDeclaredAsync)
                    {
                        var task = (Task)result;
                        HandleAsyncCase(task);
                    }
                }
                else if (field != null)
                {
                    try
                    {
                        result = ((Delegate)field.GetValue(instance)).DynamicInvoke();
                        var task = (Task)result;
                        if (task != null)
                        {
                            HandleAsyncCase(task);
                        }
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw new PreservedException(ex.InnerException);
                    }
                }
            }
            catch (PreservedException preservedException)
            {
                @case.Exceptions.Add(preservedException.OriginalException);
            }
            catch (Exception ex)
            {
                @case.Exceptions.Add(ex);
            }
        }

        static void HandleAsyncCase(Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                throw new PreservedException(ex.InnerExceptions.First());
            }
        }

        static void ThrowForUnsupportedAsyncVoid()
        {
            throw new NotSupportedException(
                "Async void methods are not supported. Declare async methods with a " +
                "return type of Task to ensure the task actually runs to completion.");
        }
    }
}