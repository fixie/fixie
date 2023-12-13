using Fixie.Reports;

namespace Fixie;

public class ReportCollection
{
    internal List<IReport> Items { get; }

    internal ReportCollection() => Items = [];

    public void Add(IReport report)
        => Items.Add(report);

    public void Add<TReport>() where TReport : IReport, new()
        => Add(new TReport());
}