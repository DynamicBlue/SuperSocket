using Dynamic.Core.Service;
using Microsoft.Extensions.Options;
using SuperSocket.ProtoBase.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Server.Runtime
{
    public class CSuperSocketService<TReceivePackageInfo> : SuperSocketService<TReceivePackageInfo> where TReceivePackageInfo : class
    {
        protected static IOptions<ServerOptions> GetServerCfgOptions()
        {
            var serverOptionsValue = IocUnity.Get<ServerOptions>();
            var serverOptions = new ServerConfigOptions<ServerOptions>(serverOptionsValue);
            return serverOptions;
        }
        public CSuperSocketService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions, IChannelCreatorFactory channelCreatorFactory) : base(serviceProvider, GetServerCfgOptions(),  channelCreatorFactory)
        {
          
        }
    }
}
