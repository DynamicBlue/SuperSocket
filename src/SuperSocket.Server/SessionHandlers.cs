using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    class SessionHandlers<TSessionData>
    {
        public Func<IAppSession<TSessionData>, ValueTask> Connected { get; set; }

        public Func<IAppSession<TSessionData>, ValueTask> Closed { get; set; }
    }
}