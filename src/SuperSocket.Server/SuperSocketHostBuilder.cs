using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamic.Core.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.ProtoBase.Config;
using SuperSocket.Server;
using SuperSocket.Server.Extions;
using SuperSocket.Server.Runtime;

namespace SuperSocket
{
    public class SuperSocketHostBuilder<TSessionData,TReceivePackage> : ISuperSocketHostBuilder<TReceivePackage>, IHostBuilder
    {

        protected HostBuilder _hostBuilder;

        public SuperSocketHostBuilder()
        {
            _hostBuilder = new HostBuilder();
        }

        public IDictionary<object, object> Properties => _hostBuilder.Properties;

        public virtual IHost Build()
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
                    && typeof(SuperSocketService<TSessionData, TReceivePackage>).IsAssignableFrom(sd.ImplementationType)))
                {
                    RegisterDefaultHostedService(services);
                }
            }).Build();
        }

        protected virtual void RegisterDefaultHostedService(IServiceCollection services)
        {
            services.AddHostedService<SuperSocketService<TSessionData, TReceivePackage>>();
        }

        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureAppConfiguration(configureDelegate);
            return this;
        }

        public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureContainer<TContainerBuilder>(configureDelegate);
            return this;
        }

        public SuperSocketHostBuilder<TSessionData, TReceivePackage> ConfigureDefaults()
        {
            var hostBuilder = this.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                });

            return this.ConfigureServices((hostCtx, services) =>
                {
                    services.AddOptions();
                    services.Configure<ServerOptions>(hostCtx.Configuration.GetSection("serverOptions"));
                });
        }
        public SuperSocketHostBuilder<TSessionData, TReceivePackage> ConfigureSimpleConfig(SimpleSocketConfig simpleSocketConfig)
        {

            ServerOptions serverOptions = simpleSocketConfig.ToServerOptions();

            return this.ConfigureServices((hostCtx, services) =>
            {
                services.AddOptions();
                IocUnity.AddSingleton(serverOptions);
                IOptions<ServerOptions> options = new ServerConfigOptions<ServerOptions>(serverOptions);
                IocUnity.AddSingleton<IOptions<ServerOptions>>(options);
            }) as SuperSocketHostBuilder<TSessionData,TReceivePackage>;

        }

        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureHostConfiguration(configureDelegate);
            return this;
        }

        IHostBuilder IHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            return ConfigureServices(configureDelegate);
        }

        public SuperSocketHostBuilder<TSessionData, TReceivePackage> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _hostBuilder.ConfigureServices(configureDelegate);
            return this;
        }



        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            _hostBuilder.UseServiceProviderFactory<TContainerBuilder>(factory);
            return this;
        }

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            _hostBuilder.UseServiceProviderFactory<TContainerBuilder>(factory);
            return this;
        }

        public virtual SuperSocketHostBuilder<TSessionData, TReceivePackage> UsePipelineFilter<TPipelineFilter>()
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IPipelineFilterFactory<TReceivePackage>, DefaultPipelineFilterFactory<TReceivePackage, TPipelineFilter>>();
            });
        }

        public virtual SuperSocketHostBuilder<TSessionData, TReceivePackage> UsePipelineFilterFactory<TPipelineFilterFactory>()
            where TPipelineFilterFactory : class, IPipelineFilterFactory<TReceivePackage>
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IPipelineFilterFactory<TReceivePackage>, TPipelineFilterFactory>();
            });
        }

        public virtual SuperSocketHostBuilder<TSessionData, TReceivePackage> UseHostedService<THostedService>()
            where THostedService : SuperSocketService<TSessionData, TReceivePackage>
        {
            return this.ConfigureServices((ctx, services) =>
            {
                services.AddHostedService<THostedService>();
            });
        }
        

        public virtual SuperSocketHostBuilder<TSessionData, TReceivePackage> UsePackageDecoder<TPackageDecoder>()
            where TPackageDecoder : class, IPackageDecoder<TReceivePackage>
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<IPackageDecoder<TReceivePackage>, TPackageDecoder>();
                }
            );
        }

        public virtual SuperSocketHostBuilder<TSessionData,TReceivePackage> ConfigureErrorHandler(Func<IAppSession<TSessionData>, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler)
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    services.AddSingleton<Func<IAppSession<TSessionData>, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);
                }
            );
        }

        public virtual SuperSocketHostBuilder<TSessionData,TReceivePackage> UsePackageHandler(Func<IAppSession<TSessionData>, TReceivePackage, ValueTask> packageHandler, Func<IAppSession<TSessionData>, PackageHandlingException<TReceivePackage>, ValueTask<bool>> errorHandler = null)
        {
            return this.ConfigureServices(
                (hostCtx, services) =>
                {
                    if (packageHandler != null)
                        services.AddSingleton<IPackageHandler<TSessionData,TReceivePackage>>(new DelegatePackageHandler<TSessionData,TReceivePackage>(packageHandler));

                    if (errorHandler != null)
                        services.AddSingleton<Func<IAppSession<TSessionData>, PackageHandlingException<TReceivePackage>, ValueTask<bool>>>(errorHandler);
                }
            );
        }
    }

    public static class SuperSocketHostBuilder<TSessionData>
    {
        public static SuperSocketHostBuilder<TSessionData,TReceivePackage> Create<TReceivePackage>()
            where TReceivePackage : class
        {
            return new SuperSocketHostBuilder<TSessionData,TReceivePackage>()
                .ConfigureDefaults();
        }
        public static SuperSocketHostBuilder<TSessionData,TReceivePackage> Create<TReceivePackage>(SimpleSocketConfig simpleSocketConfig)
         where TReceivePackage : class
        {
            return new SuperSocketHostBuilder<TSessionData,TReceivePackage>()
                .ConfigureSimpleConfig(simpleSocketConfig);
        }

        public static SuperSocketHostBuilder<TSessionData,TReceivePackage> Create<TReceivePackage, TPipelineFilter>()
            where TReceivePackage : class
            where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return new SuperSocketHostBuilder<TSessionData,TReceivePackage>()
                .ConfigureDefaults()
                .UsePipelineFilter<TPipelineFilter>();
        }
        public static SuperSocketHostBuilder<TSessionData,TReceivePackage> Create<TReceivePackage, TPipelineFilter>(SimpleSocketConfig simpleSocketConfig)
         where TReceivePackage : class
         where TPipelineFilter : IPipelineFilter<TReceivePackage>, new()
        {
            return new SuperSocketHostBuilder<TSessionData,TReceivePackage>()
                .ConfigureSimpleConfig(simpleSocketConfig)
                .UsePipelineFilter<TPipelineFilter>();
        }
    }
}
