using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SuperSocket.ProtoBase;
using SuperSocket.Server;


namespace SuperSocket
{
    public static class HostBuilderExtensions
    {
        public static ISuperSocketHostBuilder AsSuperSocketBuilder(this IHostBuilder hostBuilder)
        {
            return hostBuilder as ISuperSocketHostBuilder;
        }

        public static ISuperSocketHostBuilder UseMiddleware<TMiddleware>(this ISuperSocketHostBuilder builder)
            where TMiddleware : class, IMiddleware
        {
            return builder.ConfigureServices((ctx, services) =>
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IMiddleware, TMiddleware>());
            }).AsSuperSocketBuilder();
        }
    }
}
