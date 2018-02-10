namespace ZetaIpc.Runtime.Server
{
    using HttpServer;
    using HttpServer.FormDecoders;
    using System.IO;
    using System.Text;

    public class MyFormDecoder : IFormDecoder
    {
        HttpForm IFormDecoder.Decode(Stream stream, string contentType, Encoding encoding)
        {
            var form = new HttpForm();
            return form;
        }

        bool IFormDecoder.CanParse(string contentType)
        {
            return true;
        }
    }
}