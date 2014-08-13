namespace TestClient
{
    using System;
    using System.Threading;
    using ZetaIpc.Runtime.Client;

    internal static class Program
    {
        private static void Main()
        {
            var c = new IpcClient();
            c.Initialize(12345);

            Console.WriteLine("Started client.");

            var rep = c.Send("Hello");
            Console.WriteLine("Received: " + rep);

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}