namespace Fixie.VisualStudio.TestAdapter
{
    using System;

    [Serializable]
    public sealed class Result
    {
        public string FullyQualifiedName { get; set; }
        public string DisplayName { get; set; }
        public string Outcome { get; set; }
        public TimeSpan Duration { get; set; }
        public string Output { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackTrace { get; set; }
    }
}