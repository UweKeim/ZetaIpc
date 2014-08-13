namespace ZetaIpc.Runtime.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;
    using HttpServer;

    /// <summary>
    /// Simple HTTP-based server to receive strings from the IpcClient and send back
    /// strings in response.
    /// </summary>
    public class IpcServer
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());
        private static readonly List<int> ReservedPorts = new List<int>();
        private string _localHost;
        private HttpServer _server;

        static IpcServer()
        {
            AppDomain.CurrentDomain.AssemblyResolve +=
                (sender, args) =>
                {
                    var resourceName =
                        string.Format(
                            @"ZetaIpc.Runtime.EmbeddedResources.{0}.dll",
                            new AssemblyName(args.Name).Name);

                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    {
                        if (stream == null) return null;

                        var assemblyData = new Byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);

                        return Assembly.Load(assemblyData);
                    }
                };
        }

        private string Address { get; set; }
        public int Port { get; set; }

        /// <summary>
        /// Start listening at 127.0.0.1:port.
        /// </summary>
        public void Start(int port = 0)
        {
            if (_server != null) throw new Exception("Server already initialized.");

            Address = getLocalHost();
            Port = port <= 0 ? getFreePort() : port;

            _server = new HttpServer(new MyLogWriter());

            _server.ExceptionThrown +=
                (source, exception) => { throw new Exception("Error during server processing.", exception); };

            _server.FormDecoderProviders.Add(new MyFormDecoder());
            _server.Add(new MyModule(this));
            _server.Start(IPAddress.Loopback, Port);

            Trace.WriteLine(
                string.Format(
                    @"[Web server] Started local web server for URL '{0}'.",
                    baseUrl));
        }

        /// <summary>
        /// Stop listening, free resources.
        /// </summary>
        public void Stop()
        {
            if (_server != null)
            {
                var listener = _server;
                _server = null;
                listener.Stop();
            }
        }

        /// <summary>
        /// Being called when the server receives a string.
        /// </summary>
        public event EventHandler<ReceivedRequestEventArgs> ReceivedRequest;

        protected virtual string OnReceivedRequest(string request)
        {
            var h = ReceivedRequest;
            if (h != null)
            {
                var args = new ReceivedRequestEventArgs(request);
                h(this, args);
                if (args.Handled) return args.Response;
            }

            // Default.
            return null;
        }

        private string baseUrl
        {
            get { return string.Format(@"http://{0}:{1}/", Address, Port); }
        }

        private string getLocalHost()
        {
            // Try to use something without dots, if available.
            // http://serverfault.com/questions/19820/internet-explorer-not-bypassing-proxy-for-local-addresses/19916#19916

            if (_localHost == null)
            {
                // Default.
                _localHost = @"127.0.0.1";

                try
                {
                    var lh = Dns.GetHostEntry(@"localhost");
                    if (lh.AddressList.Length > 0)
                    {
                        foreach (var address in lh.AddressList)
                        {
                            if (address.ToString() == @"127.0.0.1")
                            {
                                _localHost = @"localhost";
                                break;
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                    // Do nothing, use default, set above.
                }
            }

            return _localHost;
        }

        private static int getFreePort()
        {
            for (var i = 0; i < 10; ++i)
            {
                var port = Random.Next(9000, 15000);
                if (isPortFree(port))
                {
                    ReservedPorts.Add(port);
                    return port;
                }
            }

            throw new Exception("Unable to acquire free port.");
        }

        private static bool isPortFree(int port)
        {
            if (ReservedPorts.Contains(port))
            {
                return false;
            }
            else
            {
                // http://stackoverflow.com/a/570126/107625

                var globalProperties = IPGlobalProperties.GetIPGlobalProperties();
                var informations = globalProperties.GetActiveTcpListeners();

                return informations.All(information => information.Port != port);
            }
        }

        private static byte[] getBytesWithBom(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        internal void CheckSendText(
            IHttpRequest request,
            IHttpResponse response)
        {
            //if (!string.IsNullOrEmpty(text))
            {
                var requestText = getText(request);
                var responseText = OnReceivedRequest(requestText);

                response.ContentType = @"text/html";

                if (!string.IsNullOrEmpty(request.Headers[@"if-Modified-Since"]))
                {
#pragma warning disable 162
                    response.Status = HttpStatusCode.OK;
#pragma warning restore 162
                }

                addNeverCache(response);

                if (request.Method != @"Headers" && response.Status != HttpStatusCode.NotModified)
                {
                    Trace.WriteLine(
                        string.Format(
                            @"[Web server] Sending text for URL '{0}': '{1}'.",
                            request.Uri.AbsolutePath,
                            responseText));

                    var buffer2 = getBytesWithBom(responseText);

                    response.ContentLength = buffer2.Length;
                    response.SendHeaders();

                    response.SendBody(buffer2, 0, buffer2.Length);
                }
                else
                {
                    response.ContentLength = 0;
                    response.SendHeaders();

                    Trace.WriteLine(@"[Web server] Not sending.");
                }
            }
        }

        private string getText(IHttpRequest request)
        {
            var bytes = request.GetBody();
            if (bytes == null) return string.Empty;
            return Encoding.UTF8.GetString(bytes);
        }

        private static void addNeverCache(IHttpResponse response)
        {
            response.AddHeader(@"Last-modified", new DateTime(2005, 1, 1).ToUniversalTime().ToString(@"r"));

            response.AddHeader(@"Cache-Control", @"no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
            response.AddHeader(@"Pragma", @"no-cache");
        }

        public void SendKeepAliveReply(IHttpRequest request, IHttpResponse response)
        {
            response.Status = HttpStatusCode.OK;
            response.AddHeader(@"Connection", @"Keep-Alive");
            response.SendHeaders();
            response.SendBody(Encoding.UTF8.GetBytes(string.Empty));
        }
    }
}