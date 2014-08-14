# Zeta IPC

A tiny .NET library to do inter-process communication (IPC) between different processes on the same machine.

## Background

First trying [ZeroMQ](https://github.com/zeromq/netmq) to do some very small IPC between two processes on the same machine, I failed and didn't 
bother to dig deeper. Instead I used the phantastic [C# WebServer project](https://webserver.codeplex.com/) and quickly
assembled some small wrapper.

I intentionally implemented simple string send and receive methods, everything else is out of scope of
the library. E.g. you could use [Json.NET](http://james.newtonking.com/json) to transfer JSON within the strings between the client and the
server.

## Using the server

To use the server (i.e. the "thing" that listens for incoming request and answers them), do something like:

    var s = new IpcServer();
    s.Start(12345); // Passing no port selects a free port automatically.

    Console.WriteLine("Started server on port {0}.", s.Port);

    s.ReceivedRequest += (sender, args) =>
    {
        args.Response = "I've got: " + args.Request;
        args.Handled = true;
    };

This starts a new background thread and continues execution.

Later, simply call

    s.Stop();

to stop the server again.

## Using the client

To use the client (i.e. the "thing" that can send texts to the server), do something like:

    var c = new IpcClient();
    c.Initialize(12345);

    Console.WriteLine("Started client.");

    var rep = c.Send("Hello");
    Console.WriteLine("Received: " + rep);

## Other usages

If you want a bi-directional communication between the server and client that can be started
by both the client and the server, simply use the above code of the server on the client
and the code of the client on the server (use different ports, of course).

This gives you two applications, each of them being server and client at the same time.

## Notes

- The web server is included inside the ZetaIpc.dll, no need to ship additional DLLs.
- Also available as a [NuGet package](https://www.nuget.org/packages/ZetaIpc).
- I'm using this library in our (Test and Requirments Management application)(http://www.zeta-test.com).