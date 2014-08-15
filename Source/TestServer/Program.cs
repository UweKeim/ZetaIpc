namespace TestServer
{
    using System;
    using System.Threading;
    using ZetaIpc.Runtime.Server;

    internal static class Program
    {
        private static void Main()
        {
            var s = new IpcServer();
            s.Start(12345);

            Console.WriteLine("Started server.");

            s.ReceivedRequest += (sender, args) =>
            {
                Console.WriteLine("Received: " + args.Request);
                args.Response = "Super: " + args.Request;
                args.Handled = true;

                throw new Exception("Test");
            };

            while (true)
            {
                Thread.Sleep(1000);
            }

            s.Stop();
        }
    }
}