using Microsoft.Extensions.Options;
using SuperSocket;
using SuperSocket.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSuperSocket.DemoServer
{
    public class DemoService<TSessionData,TReceivePackageInfo> : SuperSocketService<TSessionData,TReceivePackageInfo> where TReceivePackageInfo : class
    {
        public DemoService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions, IChannelCreatorFactory channelCreatorFactory) : base(serviceProvider, serverOptions, channelCreatorFactory)
        {

        }

        
    }
}
