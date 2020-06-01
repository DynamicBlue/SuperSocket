using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using System.Net;
using System.Net.Sockets;

using Dynamic.Core.Log;

namespace SuperSocket
{
    public static class SuperSocketExtensions
    {
        static ILogger _Logger = LoggerManager.GetLogger("SuperSocketExtensions");
        public static async ValueTask<bool> ActiveConnect(this IServer server, EndPoint remoteEndpoint)
        {
            var socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await socket.ConnectAsync(remoteEndpoint);
                await (server as IChannelRegister).RegisterChannel(socket);
                return true;
            }
            catch (Exception e)
            {
                _Logger.Error($"Failed to connect to {remoteEndpoint}¡¾{e.ToString()}¡¿");
                return false;
            }
        }

        public static ILogger GetDefaultLogger<TSessionData>(this IAppSession<TSessionData> session)
        {
            return _Logger;
        }
    }
}
