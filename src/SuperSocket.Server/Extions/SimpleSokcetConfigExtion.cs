using SuperSocket.ProtoBase.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Server.Extions
{
    public static class SimpleSokcetConfigExtion
    {
        public static ServerOptions ToServerOptions(this SimpleSocketConfig simpleSocketConfig)
        {
            return ProtocalsExtonds.ConvertSocketServiceCfg(simpleSocketConfig);
        }
    }
}
