using System;
using ZetaIpc.Runtime.Client;

namespace TestClient
{
  class Program
  {
    static void Main(string[] args)
    {
      IpcClient client = new IpcClient();
      client.Initialize(12345);

      Console.WriteLine("Started client");

      string rep = client.Send("Hello");
      Console.WriteLine($"Received: {rep}");
      Console.ReadLine();
    }
  }
}