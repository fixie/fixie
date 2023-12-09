using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Fixie.Internal;

sealed class RecordingWriter : TextWriter
{
    bool recording;
    readonly TextWriter original;
    readonly StringWriter copy;

    public RecordingWriter(TextWriter original)
    {
        this.original = original;

        copy = new StringWriter(original.FormatProvider)
        {
            NewLine = original.NewLine
        };

        recording = false;
    }

    public void StartRecording()
    {
        recording = true;
    }

    public void StopRecording()
    {
        recording = false;
        copy.GetStringBuilder().Clear();
    }

    public void StopRecording(out string recordedOutput)
    {
        recordedOutput = copy.ToString();
        StopRecording();
    }

    public override IFormatProvider FormatProvider => original.FormatProvider;

    public override Encoding Encoding => original.Encoding;

    [AllowNull]
    public override string NewLine
    {
        get => original.NewLine;
        set
        {
            original.NewLine = value;
            copy.NewLine = value;
        }
    }

    public override void Write(char value)
    {
        original.Write(value);
        if (recording) copy.Write(value);
    }

    public override void Write(char[]? buffer)
    {
        original.Write(buffer);
        if (recording) copy.Write(buffer);
    }

    public override void Write(char[] buffer, int index, int count)
    {
        original.Write(buffer, index, count);
        if (recording) copy.Write(buffer, index, count);
    }

    public override void Write(ReadOnlySpan<char> buffer)
    {
        original.Write(buffer);
        if (recording) copy.Write(buffer);
    }

    public override void Write(string? value)
    {
        original.Write(value);
        if (recording) copy.Write(value);
    }

    public override void Flush()
    {
        original.Flush();
        copy.Flush();
    }

    public override void Close()
    {
        original.Close();
        copy.Close();
    }

    bool disposed;
    protected override void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing)
        {
            original.Dispose();
            copy.Dispose();
        }

        disposed = true;

        base.Dispose(disposing);
    }
}