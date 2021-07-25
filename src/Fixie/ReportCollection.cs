namespace Fixie
{
    using System.Collections.Generic;
    using Reports;

    public class ReportCollection
    {
        internal List<IReport> Items { get; } = new List<IReport>();

        public void Add(IReport report)
            => Items.Add(report);
    }
}