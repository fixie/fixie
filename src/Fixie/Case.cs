using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Fixie
{
    public class Case
    {
        readonly object[] parameters;

        public Case(MethodInfo caseMethod, params object[] parameters)
        {
            this.parameters = parameters != null && parameters.Length == 0 ? null : parameters;
            Class = caseMethod.ReflectedType;

            Method = caseMethod.IsGenericMethodDefinition
                         ? caseMethod.MakeGenericMethod(GenericArgumentResolver.ResolveTypeArguments(caseMethod, parameters))
                         : caseMethod;

            Name = GetName();
        }

        string GetName()
        {
            var name = Class.FullName + "." + Method.Name;

            if (Method.IsGenericMethod)            
                name = string.Format("{0}<{1}>", name, string.Join(", ", Method.GetGenericArguments().Select(x => x.FullName)));

            if (parameters != null && parameters.Length > 0)
                name = string.Format("{0}({1})", name, string.Join(", ", parameters.Select(x => x.ToDisplayString())));

            return name;
        }

        public string Name { get; private set; }
        public Type Class { get; private set; }
        public MethodInfo Method { get; private set; }        

        public virtual void Execute(object instance, CaseExecution caseExecution)
        {
            try
            {
                bool isDeclaredAsync = Method.IsAsync();

                if (isDeclaredAsync && Method.IsVoid())
                    ThrowForUnsupportedAsyncVoid();

                object result;
                try
                {
                    result = Method.Invoke(instance, parameters);
                }
                catch (TargetInvocationException exception)
                {
                    throw new PreservedException(exception.InnerException);
                }

                if (isDeclaredAsync)
                {
                    var task = (Task)result;
                    try
                    {
                        task.Wait();
                    }
                    catch (AggregateException exception)
                    {
                        throw new PreservedException(exception.InnerExceptions.First());
                    }

                    if (Method.ReturnType.IsGenericType)
                    {
                        var property = task.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);

                        result = property.GetValue(task, null);
                    }
                    else
                    {
                        result = null;
                    }
                }

                caseExecution.Result = result;
            }
            catch (Exception exception)
            {
                caseExecution.Fail(exception);
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
