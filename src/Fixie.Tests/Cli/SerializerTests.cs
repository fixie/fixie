namespace Fixie.Tests.Cli
{
    using Fixie.Cli;
    using Assertions;

    public class SerializerTests
    {
        public void ShouldConcatenateQuotedValues()
        {
            Serialize("abc", "def", "ghi")
                .ShouldEqual("\"abc\" \"def\" \"ghi\"");

            Serialize("abc", "def ghi jkl", "mno pqr")
                .ShouldEqual("\"abc\" \"def ghi jkl\" \"mno pqr\"");

            Serialize("abc", "def\tghi\njkl", "mno pqr")
                .ShouldEqual("\"abc\" \"def\tghi\njkl\" \"mno pqr\"");

            Serialize("argument1", "argument 2", "\\some\\path with\\spaces")
                .ShouldEqual("\"argument1\" \"argument 2\" \"\\some\\path with\\spaces\"");
        }

        public void ShouldPreserveValuesContainingQuotesBySlashEscapingQuotes()
        {
            Serialize("argument1", "she said, \"you had me at hello\"", "\\some\\path with\\spaces")
                .ShouldEqual("\"argument1\" \"she said, \\\"you had me at hello\\\"\" \"\\some\\path with\\spaces\"");
        }

        public void ShouldPreserveValuesEndingWithSlashes()
        {
            Serialize("argument1", "argumentEndingWithSlash\\", "\\some\\path ending with\\slash\\")
                .ShouldEqual("\"argument1\" \"argumentEndingWithSlash\\\\\" \"\\some\\path ending with\\slash\\\\\"");
        }

        static string Serialize(params string[] arguments)
            => CommandLine.Serialize(arguments);
    }
}