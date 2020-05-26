using Dynamic.Core.Log;
using Microsoft.Extensions.Hosting;
using NLog;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.ProtoBase.Config;
using System;
using System.Threading.Tasks;

namespace CSuperSocket.DemoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
             Init().GetAwaiter().GetResult();
        }
        static async Task Init()
        {
            LoggerManager.InitLogger(new LogConfig());
            SimpleSocketConfig simpleSocketConfig = new SimpleSocketConfig() { 
               Port=6868
            };

            var host = SuperSocketHostBuilder.Create<StringPackageInfo, CommandLinePipelineFilter>(simpleSocketConfig)
               // .UseHostedService<>()
                .Build();

            await host.RunAsync();
        }
    }
}
