namespace Fixie
{
    using Execution.Behaviors;

    public static class CaseExtensions
    {
        public static void Execute(this Case @case, object instance)
            => new InvokeMethod().Execute(@case, instance);
    }
}