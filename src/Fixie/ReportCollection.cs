namespace Fixie
{
    using System.Collections.Generic;
    using Reports;

    public class ReportCollection
    {
        internal List<IReport> Items { get; }

        internal ReportCollection() => Items = new List<IReport>();

        public void Add(IReport report)
            => Items.Add(report);

        public void Add<TReport>() where TReport : IReport, new()
            => Add(new TReport());
    }
}