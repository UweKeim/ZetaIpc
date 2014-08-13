namespace ZetaIpc.Runtime.Client
{
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
        public void Initialize(int port)
        {
            _port = port;
        }

        /// <summary>
        /// Sends a string to the server, gets the response string back.
        /// Works synchronously, so better call it from within a background
        /// thread to keep the UI responsive.
        /// </summary>
        public string Send(string request)
        {
            using (var wc = new MyWebClient())
            {
                return wc.UploadString(url, @"POST", request ?? string.Empty);
            }
        }

        private string url
        {
            get { return string.Format(@"http://127.0.0.1:{0}", _port); }
        }
    }
}