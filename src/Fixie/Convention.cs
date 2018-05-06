namespace Fixie
{
    public abstract class Convention : Discovery, Lifecycle
    {
        /// <summary>
        /// Defines a test class lifecycle, to be executed once per test class.
        /// </summary>
        public virtual void Execute(TestClass testClass)
        {
            testClass.RunCases(@case =>
            {
                var instance = testClass.Construct();

                @case.Execute(instance);

                instance.Dispose();
            });
        }
    }
}