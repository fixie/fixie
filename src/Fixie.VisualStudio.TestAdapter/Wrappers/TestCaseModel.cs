namespace Fixie.VisualStudio.TestAdapter.Wrappers
{
    using System;

    [Serializable]
    public class TestCaseModel
    {
        public string MethodGroup { get; set; }
        public string AssemblyPath { get; set; }
        public string CodeFilePath { get; set; }
        public int? LineNumber { get; set; }
    }
}