namespace Fixie
{
    using System.Reflection;
    using Internal;

    public class TestMethod
    {
        readonly ExecutionRecorder recorder;
        readonly ParameterGenerator parameterGenerator;

        internal TestMethod(ExecutionRecorder recorder, ParameterGenerator parameterGenerator, MethodInfo method)
        {
            this.recorder = recorder;
            this.parameterGenerator = parameterGenerator;
            Method = method;
        }

        public MethodInfo Method { get; }
    }
}