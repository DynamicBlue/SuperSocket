using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface ISessionContainer<TSessionData>
    {
        IAppSession<TSessionData> GetSessionByID(string sessionID);

        int GetSessionCount();

        IEnumerable<IAppSession<TSessionData>> GetSessions(Predicate<IAppSession<TSessionData>> critera = null);

        IEnumerable<TAppSession> GetSessions<TAppSession>(Predicate<TAppSession> critera = null)
            where TAppSession : IAppSession<TSessionData>;
    }

    public interface IAsyncSessionContainer<TSessionData>
    {
        ValueTask<IAppSession<TSessionData>> GetSessionByIDAsync(string sessionID);

        ValueTask<int> GetSessionCountAsync();

        ValueTask<IEnumerable<IAppSession<TSessionData>>> GetSessionsAsync(Predicate<IAppSession<TSessionData>> critera = null);

        ValueTask<IEnumerable<TAppSession>> GetSessionsAsync<TAppSession>(Predicate<TAppSession> critera = null)
            where TAppSession : IAppSession<TSessionData>;
    }
}