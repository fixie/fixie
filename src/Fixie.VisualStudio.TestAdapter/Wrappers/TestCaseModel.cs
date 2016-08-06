namespace Fixie.VisualStudio.TestAdapter.Wrappers
{
    public class TestCaseModel
    {
        public string MethodGroup { get; set; }
        public string AssemblyPath { get; set; }
        public string CodeFilePath { get; set; }
        public int? LineNumber { get; set; }
    }
}