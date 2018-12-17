using System.IO;
using System.Text;
using HttpServer;
using HttpServer.FormDecoders;

namespace ZetaIpc.Runtime.Server
{
    public class MyFormDecoder : IFormDecoder
    {
        public HttpForm Decode(Stream stream, string contentType, Encoding encoding)
        {
            HttpForm form = new HttpForm();
            return form;
        }

        public bool CanParse(string contentType)
        {
            return true;
        }
    }
}