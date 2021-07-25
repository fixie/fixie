﻿namespace Fixie.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Reports;

    class Bus
    {
        readonly TextWriter console;
        readonly List<IReport> reports;

        public Bus(TextWriter console, IReport report)
            : this(console, new[] { report })
        {
        }

        public Bus(TextWriter console, IReadOnlyList<IReport> reports)
        {
            this.console = console;
            this.reports = new List<IReport>(reports);
        }

        public async Task PublishAsync<TMessage>(TMessage message) where TMessage : Message
        {
            foreach (var report in reports)
            {
                try
                {
                    if (report is IHandler<TMessage> asyncHandler)
                        await asyncHandler.Handle(message);
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
}