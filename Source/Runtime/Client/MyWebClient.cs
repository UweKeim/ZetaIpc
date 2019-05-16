using System;
using System.Net;

namespace ZetaIpc.Runtime.Client
{
    internal class MyWebClient : WebClient
    {
        private readonly int _timeoutMilliSeconds;

        public MyWebClient(int timeoutMilliSeconds)
        {
            _timeoutMilliSeconds = timeoutMilliSeconds;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            // http://stackoverflow.com/questions/896207/c-sharp-get-rid-of-connection-header-in-webclient

            WebRequest request = base.GetWebRequest(address);

            if (request is HttpWebRequest r) r.KeepAlive = false;

            if (request != null && _timeoutMilliSeconds > 0)
                request.Timeout = _timeoutMilliSeconds;

            return request;
        }    
    }
}