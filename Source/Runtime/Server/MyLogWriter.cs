using System.Diagnostics;
using HttpServer;

namespace ZetaIpc.Runtime.Server
{
    public class MyLogWriter : ILogWriter
    {
        public void Write(object source, LogPrio priority, string message)
        {
            TraceLog(priority.ToString(), message);
        }

        private static void TraceLog(string type, string message)
        {
            Trace.WriteLine($@"[Web server, {type}] {message}");
        }
    }
}