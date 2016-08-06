namespace Fixie.VisualStudio.TestAdapter.Wrappers
{
    using System;

    [Serializable]
    public class TestResultModel
    {
        public TestCaseModel TestCase { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public TimeSpan Duration { get; set; }
        public string Output { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackTrace { get; set; }
    }
}