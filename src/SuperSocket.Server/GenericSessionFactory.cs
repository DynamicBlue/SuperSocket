using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    public class GenericSessionFactory<TSessionData,TSession> : ISessionFactory<TSessionData>
        where TSession : AppSession<TSessionData>, new()
    {
        public Type SessionType => typeof(TSession);

        public IAppSession<TSessionData> Create()
        {
            return new TSession();
        }
    }
}