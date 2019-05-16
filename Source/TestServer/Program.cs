using System;
using System.Threading;
using ZetaIpc.Runtime.Server;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            IpcServer server = new IpcServer();
            server.Start(12345);

            Console.WriteLine("Started server");
            
            server.ReceivedRequest += ServerOnReceivedRequest;

            while (true)
            {
                Thread.Sleep(1000);    
            }
        }

        private static void ServerOnReceivedRequest(object sender, ReceivedRequestEventArgs e)
        {
            Console.WriteLine($"Received: {e.Request}");
            e.Response = $"Super: {e.Request}";
            e.Handled = true;
        }
    }
}