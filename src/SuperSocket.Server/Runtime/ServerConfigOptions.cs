using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Server.Runtime
{
    public class ServerConfigOptions<TOptions> : IOptions<TOptions> where TOptions : class, new()
    {
        private TOptions OptionsValue { get; set; }
        public ServerConfigOptions(TOptions options)
        {
            this.OptionsValue = options;
        }
        public TOptions Value => this.OptionsValue;
    }
}
