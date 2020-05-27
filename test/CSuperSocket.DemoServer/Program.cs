using CSuperSocket.DemoServer.Filter;
using Dynamic.Core.Auxiliary;
using Dynamic.Core.Extensions;
using Dynamic.Core.Log;
using Microsoft.Extensions.Hosting;
using NLog;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.ProtoBase.Config;
using SuperSocket.ProtoBase.Filter;
using SuperSocket.Server.Runtime;
using System;
using System.Buffers;
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
            SimpleSocketConfig simpleSocketConfig = new SimpleSocketConfig()
            {
                Port = 6868
            };
            long i = 0;
            var host = SuperSocketHostBuilder.Create<BinaryRequestInfo, BeginEndWraperByteFilter>(simpleSocketConfig)
                .UseHostedService<CSuperSocketService<BinaryRequestInfo>>()
                .UsePackageHandler(async (session, requestInfo) =>
                {
                    i++;

                 //   await Task.Factory.StartNew(()=> {
                        var colorf = i % 2 == 0 ? ConsoleColor.Red : ConsoleColor.Green;
                        IOHelper.WriteLine(requestInfo.OriBuffer.ToHex(),colorf);
                        if (i % 100 == 0)
                        { 
                        Console.Clear();
                        }
                    await session.SendAsync(requestInfo.Body.ToArray());
                   // });
                    
                })
                .Build();

            await host.RunAsync();
        }
    }
}
