using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Server.Runtime
{
    public class CSuperSocketHostBuilder<TSessionData,TReceivePackage> :SuperSocketHostBuilder<TSessionData,TReceivePackage> where TReceivePackage : class
    {
        public override IHost Build()
        {
            return _hostBuilder.ConfigureServices((ctx, services) =>
            {
                // if the package type is StringPackageInfo
                if (typeof(TReceivePackage) == typeof(StringPackageInfo))
                {
                    services.TryAdd(ServiceDescriptor.Singleton<IPackageDecoder<StringPackageInfo>, DefaultStringPackageDecoder>());
                }

                services.TryAdd(ServiceDescriptor.Singleton<IChannelCreatorFactory, TcpChannelCreatorFactory>());
                services.TryAdd(ServiceDescriptor.Singleton<IPackageEncoder<string>, DefaultStringEncoderForDI>());

                // if no host service was defined, just use the default one
                if (!services.Any(sd => sd.ServiceType == typeof(IHostedService)
                    && typeof(SuperSocketService<TSessionData,TReceivePackage>).IsAssignableFrom(sd.ImplementationType)))
                {
                    RegisterDefaultHostedService(services);
                }
            }).Build();
        }
    }
}
