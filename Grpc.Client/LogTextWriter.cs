using System;
using System.IO;
using System.Text;

namespace Grpc.Client;

internal class LogTextWriter : TextWriter
{
    private readonly Action<string> _handler;

    public LogTextWriter(Action<string> handler)
    {
        _handler = handler;
    }

    public override Encoding Encoding => Encoding.UTF8;

    private readonly StringBuilder _sb = new();
    private bool _skipN;
    public override void Write(char value)
    {
        if (value == '\r')
        {
            _skipN = true;
            SendLine();
            return;
        }
        if (value == '\n')
        {
            if (!_skipN)
            {
                _skipN = false;
                SendLine();
            }
            return;
        }

        _sb.Append(value);

        void SendLine()
        {
            _handler?.Invoke(_sb.ToString());
            _sb.Clear();
        }
    }
}