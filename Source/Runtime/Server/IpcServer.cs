using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using HttpServer;
using ZetaIpc.Runtime.Helper;

namespace ZetaIpc.Runtime.Server
{
    public class IpcServer :
        IDisposable
    {
        private string _localHost;
        private HttpServer.HttpServer _server;

        static IpcServer()
        {
            AppDomain.CurrentDomain.AssemblyResolve +=
                (sender, args) =>
                {
                    string resourceName =
                        $@"ZetaIpc.Runtime.EmbeddedResources.{new AssemblyName(args.Name).Name}.dll";

                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    {
                        if (stream == null) return null;

                        byte[] assemblyData = new Byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);

                        return Assembly.Load(assemblyData);
                    }
                };
        }

        private string Address { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public int Port { get; set; }

        /// <summary>
        /// Start listening at 127.0.0.1:port.
        /// </summary>
        public void Start(int port = 0)
        {
            if (_server != null) throw new Exception("Server already initialized.");

            Address = GetLocalHost();
            Port = port <= 0 ? FreePortHelper.GetFreePort() : port;

            _server = new HttpServer.HttpServer(new MyLogWriter());

            _server.ExceptionThrown +=
                (source, exception) => throw new Exception("Error during server processing.", exception);

            _server.FormDecoderProviders.Add(new MyFormDecoder());
            _server.Add(new MyModule(this));
            _server.Start(IPAddress.Loopback, Port);

            Trace.WriteLine(
                $@"[Web server] Started local web server for URL '{BaseUrl}'.");
        }

        /// <summary>
        /// Stop listening, free resources.
        /// </summary>
        public void Stop()
        {
            if (_server != null)
            {
                HttpServer.HttpServer listener = _server;
                _server = null;
                listener.Stop();
            }
        }

        /// <summary>
        /// Being called when the server receives a string.
        /// </summary>
        /// <remarks>
        /// This event is called from within a background thread (also
        /// called "worker thread"), i.e. a non-UI thread. If you are handling
        /// the event within a WinForms or WPF application and want to update
        /// UI stuff (e.g. write a text to a text box or show a message box),
        /// you have to use Control.Invoke or Control.BeginInvoke to marshall
        /// your call to the foreground thread since WinForms/WPF controls can
        /// be accessed only from within the main UI thread.
        /// </remarks>
        public event EventHandler<ReceivedRequestEventArgs> ReceivedRequest;

        protected virtual string OnReceivedRequest(string request)
        {
            EventHandler<ReceivedRequestEventArgs> h = ReceivedRequest;
            if (h != null)
            {
                ReceivedRequestEventArgs args = new ReceivedRequestEventArgs(request);
                h(this, args);
                if (args.Handled) return args.Response;
            }

            // Default.
            return null;
        }

        private string BaseUrl => $@"http://{Address}:{Port}/";

        private string GetLocalHost()
        {
            // Try to use something without dots, if available.
            // http://serverfault.com/questions/19820/internet-explorer-not-bypassing-proxy-for-local-addresses/19916#19916

            if (_localHost == null)
            {
                // Default.
                _localHost = @"127.0.0.1";

                try
                {
                    IPHostEntry lh = Dns.GetHostEntry(@"localhost");
                    if (lh.AddressList.Length > 0)
                    {
                        foreach (IPAddress address in lh.AddressList)
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

        private static byte[] GetBytesWithBom(string text)
        {
            return Encoding.UTF8.GetBytes(text ?? string.Empty);
        }

        internal void CheckSendText(
            IHttpRequest request,
            IHttpResponse response)
        {
            string requestText = GetText(request);
            string responseText;

            try
            {
                responseText = OnReceivedRequest(requestText);
            }
            catch (Exception x)
            {
                Trace.TraceError(@"Error during request handling: {0}", x);
                SendError500(response, x);
                throw;
            }

            sendReply(request, response, responseText);
        }

        private void sendReply(IHttpRequest request, IHttpResponse response, string responseText)
        {
            responseText = responseText ?? string.Empty;
            response.ContentType = @"text/html";

            if (!string.IsNullOrEmpty(request.Headers[@"if-Modified-Since"]))
            {
                response.Status = HttpStatusCode.OK;
            }

            AddNeverCache(response);

            if (request.Method != @"Headers" && response.Status != HttpStatusCode.NotModified)
            {
                Trace.WriteLine(
                    $@"[Web server] Sending text for URL '{request.Uri.AbsolutePath}': '{responseText}'.");

                byte[] buffer2 = GetBytesWithBom(responseText);

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

        private static void SendError500(IHttpResponse response, Exception exception)
        {
            response.ContentType = @"text/html";
            response.Status = HttpStatusCode.InternalServerError;

            string responseText = MakeError500ResponseText(exception);
            byte[] buffer2 = GetBytesWithBom(responseText);

            response.ContentLength = buffer2.Length;
            response.SendHeaders();

            response.SendBody(buffer2, 0, buffer2.Length);
        }

        private static string MakeError500ResponseText(Exception exception)
        {
            return new ExceptionToXmlLight(exception).ToXmlString();
        }

        private static string GetText(IHttpRequest request)
        {
            byte[] bytes = request.GetBody();
            return bytes == null ? string.Empty : Encoding.UTF8.GetString(bytes);
        }

        private static void AddNeverCache(IHttpResponse response)
        {
            response.AddHeader(@"Last-modified", new DateTime(2005, 1, 1).ToUniversalTime().ToString(@"r"));

            response.AddHeader(@"Cache-Control", @"no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
            response.AddHeader(@"Pragma", @"no-cache");
        }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global
        public void SendKeepAliveReply(IHttpRequest request, IHttpResponse response)
        {
            response.Status = HttpStatusCode.OK;
            response.AddHeader(@"Connection", @"Keep-Alive");
            response.SendHeaders();
            response.SendBody(Encoding.UTF8.GetBytes(string.Empty));
        }

        void IDisposable.Dispose()
        {
            Stop();
        }
    }
}