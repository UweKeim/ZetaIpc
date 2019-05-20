using System.Net;
using System.Net.Sockets;

namespace ZetaIpc.Runtime.Helper
{
    public static class FreePortHelper
    {
        /// <summary>
        /// Gets a free port on the current machine.
        /// </summary>
        public static int GetFreePort()
        {
            using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                sock.Bind(new IPEndPoint(IPAddress.Loopback, 0));

                return ((IPEndPoint) sock.LocalEndPoint).Port;
            }
        }
    }
}
