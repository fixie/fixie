namespace Fixie.Tests.Execution
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Versioning;
    using Assertions;

    public class ExecutionEnvironmentTests
    {
        public void ShouldEnableAccessToTestAssemblyConfigFile()
        {
            ConfigurationManager.AppSettings["CanAccessAppConfig"].ShouldEqual("true");
        }

        public void ShouldTargetFrameworkCompatibleWithQuirksModeStatus()
        {
            // Test frameworks which support both .NET 4.0 and 4.5 can encounter an issue
            // with their AppDomain setup in which test runner behavior can be negatively
            // impacted depending on whether code targets 4.0 or 4.5.  A test could fail,
            // for instance, even when the tested code is actually right.

            // Because Fixie targets a minimum of 4.5, it doesn't actually fall prey to
            // that issue. Fixie never needs to run in the presence of quirks mode.
            // However, we include this sanity check to avoid regressions.

            // See https://youtrack.jetbrains.com/issue/RSRP-412080

            var quirksAreEnabled = Uri.EscapeDataString("'") == "'";

            quirksAreEnabled.ShouldBeFalse();

            var targetFramework =
                typeof(ExecutionEnvironmentTests)
                    .Assembly()
                    .GetCustomAttributes(typeof(TargetFrameworkAttribute), true)
                    .Cast<TargetFrameworkAttribute>()
                    .Single()
                    .FrameworkName;

            quirksAreEnabled.ShouldEqual(!targetFramework.Contains("4.5"));
        }
    }
}