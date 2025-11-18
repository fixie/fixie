namespace Fixie.Internal;

class SourceLocation
{
    public required string CodeFilePath { get; init; }
    public required int LineNumber { get; init; }
}