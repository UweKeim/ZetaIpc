namespace ZetaIpc.Runtime.Client
{
    using Helper;
    using System.IO;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Simple HTTP-based client to send strings to an IpcServer instance and 
    /// get back strings in response.
    /// </summary>
    public class IpcClient
    {
        private int _port;

        /// <summary>
        /// Initialized to connect to an IcpServer running on 127.0.0.1:port.
        /// </summary>
        /// <param name="port">The port of the running server to connect to.</param>
        /// <param name="timeoutMilliSeconds">An optional timeout, if greater zero. Default is 100 seconds (100000 milliseconds). Use for long running tasks.</param>
        public void Initialize(int port, int timeoutMilliSeconds = 0)
        {
            _port = port;
            TimeoutMilliSeconds = timeoutMilliSeconds;
        }

        /// <summary>
        /// Sends a string to the server, gets the response string back.
        /// Works synchronously, so better call it from within a background
        /// thread to keep the UI responsive.
        /// </summary>
        public string Send(string request)
        {
            using (var wc = new MyWebClient(TimeoutMilliSeconds))
            {
                try
                {
                    return wc.UploadString(url, @"POST", request ?? string.Empty);
                }
                catch (WebException x)
                {
                    // Try to give user more details (which might have been
                    // marshalled from the server).

                    if (x.Status == WebExceptionStatus.ProtocolError)
                    {
                        if (x.Response is HttpWebResponse response &&
                            response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            using (var stream = response.GetResponseStream())
                            {
                                if (stream != null)
                                {
                                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                                    {
                                        var resp = reader.ReadToEnd();
                                        if (ExceptionFromXmlLight.IsSerializedException(resp))
                                        {
                                            throw new IpcClientException(new ExceptionFromXmlLight(resp), x);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    throw;
                }
            }
        }

        private string url => $@"http://127.0.0.1:{_port}";

        /// <summary>
        /// Dynamically get or set a timeout for calling the server.
        /// A value of zero indicates the default timeout of 100 seconds (100000 milliseconds).
        /// </summary>
        public int TimeoutMilliSeconds { get; set; }
    }
}