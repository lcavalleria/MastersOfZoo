using System;
namespace MozServer
{
    public class LineReceivedEventArgs : EventArgs
    {
        string line = String.Empty;

        public LineReceivedEventArgs(string line)
        {
            this.line = line;
        }

        public string Line
        {
            get
            {
                return line;
            }
        }
    }
}