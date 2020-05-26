using SuperSocket.ProtoBase.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Server.Extions
{
    public static class ProtocalsExtonds
    {
        public static ServerOptions ConvertSocketServiceCfg(this SimpleSocketConfig simpleSocketConfig)
        {
            ServerOptions serverCfg = null;
            if (simpleSocketConfig != null)
            {
                serverCfg = new ServerOptions();

                serverCfg.Name = simpleSocketConfig.Name;
                serverCfg.IdleSessionTimeOut = simpleSocketConfig.IdleSessionTimeOut;
                serverCfg.ClearIdleSessionInterval = simpleSocketConfig.ClearIdleSessionInterval;
                serverCfg.ReceiveBufferSize = simpleSocketConfig.ReceiveBufferSize;
                serverCfg.SendBufferSize = simpleSocketConfig.SendBufferSize;
                serverCfg.DefaultTextEncoding = Encoding.GetEncoding(simpleSocketConfig.TextEncoding);

                serverCfg.MaxPackageLength = simpleSocketConfig.MaxPackageLength;
                serverCfg.ReceiveBufferSize = simpleSocketConfig.ReceiveBufferSize;
                serverCfg.SendBufferSize = simpleSocketConfig.SendBufferSize;
                serverCfg.ReceiveTimeout = simpleSocketConfig.KeepAliveTime;
                serverCfg.SendTimeout = simpleSocketConfig.KeepAliveTime;

                serverCfg.Listeners = new ListenOptions[] {
                new ListenOptions(){ Ip="Any",
                Port=simpleSocketConfig.Port
                },
                };
            };
            return serverCfg;
        }
    }
}
