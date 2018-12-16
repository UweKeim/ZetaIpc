using HttpServer;
using HttpServer.HttpModules;
using HttpServer.Sessions;

namespace ZetaIpc.Runtime.Server
{
    public class MyModule : HttpModule
    {
        private readonly IpcServer _owner;
        
        public MyModule(IpcServer owner)
        {
            _owner = owner;
        }
        
        public override bool Process(IHttpRequest request, IHttpResponse response, IHttpSession session)
        {
            if (request.Connection == ConnectionType.KeepAlive)
            {
                //Comment Author: Don't want to handle it by myself, give another tool the chance to do so.   
                return false;
            }
            else
            {
                _owner.CheckSendText(request, response);
                return true;
            }
        }
    }
}