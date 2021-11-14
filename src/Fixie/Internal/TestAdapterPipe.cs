namespace Fixie.Internal;

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using static Serialization;

class TestAdapterPipe : IDisposable
{
    readonly StreamReader reader;
    readonly StreamWriter writer;

    public TestAdapterPipe(PipeStream pipeStream)
    {
        // Normal attempts to call dispose on this reader
        // and writer cause the underlying PipeStream to
        // throw ObjectDisposedException when the original
        // PipeStream's 'using' block ends. Here we allow
        // the reader and writer to be disposed of, ignoring
        // the PipeStream itself by leaving that open.
        // Then, the original using block for the PipeStream
        // is trusted to own that cleanup.

        reader = new StreamReader(pipeStream, leaveOpen: true);
        writer = new StreamWriter(pipeStream, leaveOpen: true);
    }

    const string EndOfMessage = "--End of Message--";

    public string? ReceiveMessageType()
    {
        return reader.ReadLine();
    }

    public TMessage Receive<TMessage>()
    {
        return Deserialize<TMessage>(ReceiveMessageBody());
    }

    public string ReceiveMessageBody()
    {
        var lines = new StringBuilder();

        while (true)
        {
            var line = reader.ReadLine();

            if (line == null || line == EndOfMessage)
                break;

            lines.AppendLine(line);
        }

        return lines.ToString();
    }

    public void Send<TMessage>() where TMessage: new()
    {
        Send(new TMessage());
    }

    public void Send(Exception exception)
    {
        Send(new PipeMessage.Exception(exception));
    }

    public void Send<TMessage>(TMessage message)
    {
        var messageType = typeof(TMessage).FullName!;

        writer.WriteLine(messageType);
        writer.WriteLine(Serialize(message));
        writer.WriteLine(EndOfMessage);
        writer.Flush();
    }

    public void Dispose()
    {
        reader.Dispose();
        writer.Dispose();
    }
}