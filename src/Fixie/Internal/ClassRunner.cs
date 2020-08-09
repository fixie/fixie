namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class ClassRunner
    {
        readonly ExecutionRecorder recorder;
        readonly Execution execution;
        readonly ParameterGenerator parameterGenerator;

        public ClassRunner(ExecutionRecorder recorder, ParameterGenerator parameterGenerator, Execution execution)
        {
            this.recorder = recorder;
            this.execution = execution; 
            this.parameterGenerator = parameterGenerator;
        }

        public void Run(Type testClass, IReadOnlyList<MethodInfo> testMethods, MethodInfo? targetMethod)
        {
            recorder.Start(testClass);

            var runContext = new TestClass(recorder, parameterGenerator, testClass, testMethods, targetMethod);
            
            Exception? classLifecycleFailure = null;

            try
            {
                execution.Execute(runContext);
            }
            catch (Exception exception)
            {
                classLifecycleFailure = exception;
            }

            if (classLifecycleFailure != null)
            {
                foreach (var testMethod in testMethods)
                    recorder.Fail(testMethod, classLifecycleFailure);
            }
            else if (!runContext.Invoked)
            {
                foreach (var testMethod in testMethods)
                    recorder.Skip(testMethod);
            }
            
            recorder.Complete(testClass);
        }
    }
}