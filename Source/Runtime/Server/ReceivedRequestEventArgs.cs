namespace ZetaIpc.Runtime.Server;

using System;

public class ReceivedRequestEventArgs :
    EventArgs
{
    public string Request { get; }
    public string Response { get; set; }
    public bool Handled { get; set; }

    public ReceivedRequestEventArgs(string request)
    {
        Request = request;
    }
}