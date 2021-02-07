namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Reports;

    class Bus
    {
        readonly List<Report> reports;

        public Bus(Report report)
            : this(new[] { report })
        {
        }

        public Bus(IReadOnlyList<Report> reports)
        {
            this.reports = new List<Report>(reports);
        }

        public async Task PublishAsync<TMessage>(TMessage message) where TMessage : Message
        {
            foreach (var report in reports)
            {
                try
                {
                    if (report is Handler<TMessage> handler)
                        handler.Handle(message);

                    if (report is AsyncHandler<TMessage> asyncHandler)
                        await asyncHandler.HandleAsync(message);
                }
                catch (Exception exception)
                {
                    using (Foreground.Yellow)
                        Console.WriteLine(
                            $"{report.GetType().FullName} threw an exception while " +
                            $"attempting to handle a message of type {typeof(TMessage).FullName}:");
                    Console.WriteLine();
                    Console.WriteLine(exception.ToString());
                    Console.WriteLine();
                }
            }
        }
    }
}