namespace ZetaIpc.Runtime.Client
{
    using System;
    using System.Net;
    using System.Text;

    internal sealed class MyWebClient :
        WebClient
    {
        private readonly int _timeoutMilliSeconds;

        public MyWebClient(int timeoutMilliSeconds, Encoding encoding = null)
        {
            _timeoutMilliSeconds = timeoutMilliSeconds;
            Encoding = encoding ?? Encoding.UTF8;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            // http://stackoverflow.com/questions/896207/c-sharp-get-rid-of-connection-header-in-webclient

            var request = base.GetWebRequest(address);

            if (request is HttpWebRequest r) r.KeepAlive = false;

            if (request != null && _timeoutMilliSeconds > 0)
                request.Timeout = _timeoutMilliSeconds;

            return request;
        }
    }
}