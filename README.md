# Zeta IPC

A tiny .NET library to do inter-process communication (IPC) between different processes on the same machine.

## NuGet

Get the [`ZetaIpc` NuGet package](https://www.nuget.org/packages/ZetaIpc).

## Background

First trying [ZeroMQ](https://github.com/zeromq/netmq) to do some very small IPC between two WinForms processes on the same machine, I failed and didn't 
bother to dig deeper. Instead I used the phantastic [C# WebServer project](https://webserver.codeplex.com/) and quickly
assembled some small wrapper.

I intentionally implemented only simple string send and receive methods, everything else is out of scope of
the library. E.g. you could use [Json.NET](http://james.newtonking.com/json) to transfer JSON within the strings between the client and the server.

## Using the server

To use the server (i.e. the "thing" that listens for incoming request and answers them), do something like:

```cs
var s = new IpcServer();
s.Start(12345); // Passing no port selects a free port automatically.

Console.WriteLine("Started server on port {0}.", s.Port);

s.ReceivedRequest += (sender, args) =>
{
    args.Response = "I've got: " + args.Request;
    args.Handled = true;
};
```

This starts a new background thread and continues execution.

Later, simply call

```cs
s.Stop();
```

to stop the server again.

## Using the client

To use the client (i.e. the "thing" that can send texts to the server), do something like:

```cs
var c = new IpcClient();
c.Initialize(12345);

Console.WriteLine("Started client.");

var rep = c.Send("Hello");
Console.WriteLine("Received: " + rep);
```

## Bi-directional usage

If you want a bi-directional communication between the server and client that can be started
by both the client and the server, simply use the above code of the server on the client
and the code of the client on the server (use different ports, of course).

This gives you two applications, each of them being server and client at the same time.

## How to tell the port from the client to the server?

I've developed the library to start an external application from my main application. My main application acts as 
the client and my external application as the server. 

The whole process of starting and communicating with the external application roughly follows these steps:

1. Main application is running.
1. User clicks a menu item, which requires to launch the external application.
1. By calling the `FreePortHelper.GetFreePort()` method on the main application, a free port number is being gathered.
1. The main application calls the external application (through a relative file path) and passes the free port number as a command line parameter to the external application.
1. The external application reads the so passed port number from the command line and starts an instance of `IpcServer` on this given port.
1. The main application waits for a few seconds and then uses an instance of `IpcClient` to send messages to the external application and receives messages back. If you want to wait until the server has really started and is ready, you can [use an event wait handle](http://stackoverflow.com/questions/2740038/).

## Notes

- The web server is included inside the ZetaIpc.dll, no need to ship additional DLLs.
- Also available as a [NuGet package](https://www.nuget.org/packages/ZetaIpc).
- I'm using this library in our [Test and Requirements Management application](http://www.zeta-test.com).
