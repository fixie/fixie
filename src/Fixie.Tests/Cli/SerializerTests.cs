namespace Fixie.Tests.Cli
{
    using Fixie.Cli;
    using Assertions;

    public class SerializerTests
    {
        public void ShouldConcatenateQuotedValues()
        {
            Serialize("abc", "def", "ghi")
                .ShouldBe("\"abc\" \"def\" \"ghi\"");

            Serialize("abc", "def ghi jkl", "mno pqr")
                .ShouldBe("\"abc\" \"def ghi jkl\" \"mno pqr\"");

            Serialize("abc", "def\tghi\njkl", "mno pqr")
                .ShouldBe("\"abc\" \"def\tghi\njkl\" \"mno pqr\"");

            Serialize("argument1", "argument 2", "\\some\\path with\\spaces")
                .ShouldBe("\"argument1\" \"argument 2\" \"\\some\\path with\\spaces\"");
        }

        public void ShouldPreserveValuesContainingQuotesBySlashEscapingQuotes()
        {
            Serialize("argument1", "she said, \"you had me at hello\"", "\\some\\path with\\spaces")
                .ShouldBe("\"argument1\" \"she said, \\\"you had me at hello\\\"\" \"\\some\\path with\\spaces\"");
        }

        public void ShouldPreserveValuesEndingWithSlashes()
        {
            Serialize("argument1", "argumentEndingWithSlash\\", "\\some\\path ending with\\slash\\")
                .ShouldBe("\"argument1\" \"argumentEndingWithSlash\\\\\" \"\\some\\path ending with\\slash\\\\\"");
        }

        static string Serialize(params string[] arguments)
            => CommandLine.Serialize(arguments);
    }
}