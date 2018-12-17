using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace ZetaIpc.Runtime.Helper
{
    public static class FreePortHelper
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());
        private static readonly List<int> ReservedPorts = new List<int>();

        /// <summary>
        /// Gets a free port on the current machine.
        /// </summary>
        public static int GetFreePort()
        {
            for (var i = 0; i < 10; ++i)
            {
                int port = Random.Next(9000, 15000);
                if (IsPortFree(port))
                {
                    ReservedPorts.Add(port);
                    return port;
                }
            }

            throw new Exception("Unable to acquire free port.");
        }

        private static bool IsPortFree(int port)
        {
            if (ReservedPorts.Contains(port))
            {
                return false;
            }
            else
            {
                // http://stackoverflow.com/a/570126/107625

                IPGlobalProperties globalProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] ipEndPoints = globalProperties.GetActiveTcpListeners();

                return ipEndPoints.All(information => information.Port != port);
            }
        }
    }
}