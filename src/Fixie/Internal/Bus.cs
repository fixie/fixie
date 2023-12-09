namespace Fixie.Internal;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Reports;

class Bus
{
    readonly TextWriter console;
    readonly List<IReport> reports;

    public Bus(TextWriter console, IReadOnlyList<IReport> reports)
    {
        this.console = console;
        this.reports = new List<IReport>(reports);
    }

    public async Task Publish<TMessage>(TMessage message) where TMessage : IMessage
    {
        foreach (var report in reports)
        {
            try
            {
                if (report is IHandler<TMessage> handler)
                    await handler.Handle(message);
            }
            catch (Exception exception)
            {
                using (Foreground.Yellow)
                    console.WriteLine(
                        $"{report.GetType().FullName} threw an exception while " +
                        $"attempting to handle a message of type {typeof(TMessage).FullName}:");
                console.WriteLine();
                console.WriteLine(exception.ToString());
                console.WriteLine();
            }
        }
    }
}