using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    class DefaultSessionFactory<TSessionData> : ISessionFactory<TSessionData>
    {
        public Type SessionType => typeof(AppSession<TSessionData>);

        public IAppSession<TSessionData> Create()
        {
            return new AppSession<TSessionData>();
        }
    }
}