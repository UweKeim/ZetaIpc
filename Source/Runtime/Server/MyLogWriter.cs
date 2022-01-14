namespace ZetaIpc.Runtime.Server;

using HttpServer;
using System.Diagnostics;

internal class MyLogWriter :
    ILogWriter
{
    private static void traceLog(
        string type,
        string message)
    {
        Trace.WriteLine($@"[Web server, {type}] {message}");
    }

    public void Write(object source, LogPrio priority, string message)
    {
        traceLog(priority.ToString(), message);
    }
}