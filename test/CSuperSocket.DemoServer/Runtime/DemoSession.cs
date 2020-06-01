using SuperSocket.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSuperSocket.DemoServer.Runtime
{
    public class DemoSession<TSessionData> : AppSession<TSessionData>
    {
        public int GameHallId { get; internal set; }

        public int RoomId { get; internal set; }
        protected override async ValueTask OnSessionConnectedAsync()
        {
            // do something right after the sesssion is connected
        }
     
        protected override async ValueTask OnSessionClosedAsync(EventArgs e)
        {
            // do something right after the sesssion is closed
        }
    }
}
