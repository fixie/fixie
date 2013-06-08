using System.Runtime.CompilerServices;
using System.Text;
using Should;

namespace Fixie.Samples
{
    public static class StringBuilderExtensions
    {
        public static void WhereAmI(this StringBuilder log, [CallerMemberName] string method = null)
        {
            log.AppendLine(method);
        }

        public static void ShouldHaveLines(this StringBuilder log, params string[] expected)
        {
            var expectation = new StringBuilder();

            foreach (var line in expected)
                expectation.AppendLine(line);

            log.ToString().ShouldEqual(expectation.ToString());
        }
    }
}