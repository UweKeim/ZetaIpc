namespace ZetaIpc.Runtime.Server
{
    using System.Diagnostics;
    using HttpServer;

    internal class MyLogWriter :
        ILogWriter
    {
        private static void traceLog(
            string type,
            string message)
        {
            Trace.WriteLine(
                string.Format(
                    @"[Web server, {0}] {1}",
                    type,
                    message));
        }

        public void Write(object source, LogPrio priority, string message)
        {
            traceLog(priority.ToString(), message);
        }
    }
}