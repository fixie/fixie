using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Fixie.VisualStudio.TestAdapter
{
    public class VsOutputWindow : TextWriter
    {
        readonly IMessageLogger logger;
        string lineBuffer;

        public VsOutputWindow(IMessageLogger logger)
        {
            this.logger = logger;
            lineBuffer = "";
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void Write(char value)
        {
            if (lineBuffer == null)
                lineBuffer = "";
            lineBuffer += value;
            if (lineBuffer.EndsWith(Environment.NewLine))
            {
                lineBuffer = lineBuffer.Substring(0, lineBuffer.Length - Environment.NewLine.Length);
                Flush();
            }
        }

        public override void Flush()
        {
            if (lineBuffer != null)
                logger.Info("[Console] " + lineBuffer);
            lineBuffer = null;
        }
    }
}