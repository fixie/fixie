namespace Fixie.VisualStudio.TestAdapter
{
    using System;

    [Serializable]
    public sealed class Test
    {
        public string FullyQualifiedName { get; set; }
        public string DisplayName { get; set; }
        public string CodeFilePath { get; set; }
        public int? LineNumber { get; set; }
    }
}