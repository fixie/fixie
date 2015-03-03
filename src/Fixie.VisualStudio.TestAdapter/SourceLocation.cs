namespace Fixie.VisualStudio.TestAdapter
{
    public class SourceLocation
    {
        public SourceLocation(string codeFilePath, int lineNumber)
        {
            CodeFilePath = codeFilePath;
            LineNumber = lineNumber;
        }

        public string CodeFilePath { get; private set; }
        public int LineNumber { get; private set; }
    }
}