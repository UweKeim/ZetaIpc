using System;
using System.Net;

namespace ZetaIpc.Runtime.Client
{
    public class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            // http://stackoverflow.com/questions/896207/c-sharp-get-rid-of-connection-header-in-webclient

            var request = base.GetWebRequest(address);

            if (request is HttpWebRequest r) r.KeepAlive = false;
            return request;
        }    
    }
}