using System.Configuration;
using Should;

namespace Fixie.Tests.Execution
{
    public class ExecutionEnvironmentTests
    {
        public void ShouldEnableAccessToTestAssemblyConfigFile()
        {
            ConfigurationManager.AppSettings["CanAccessAppConfig"].ShouldEqual("true");
        }
    }
}