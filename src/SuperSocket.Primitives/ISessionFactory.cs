using System;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface ISessionFactory<TSessionData>
    {
        IAppSession<TSessionData> Create();

        Type SessionType { get; }
    }
}