using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public class AsyncToSyncSessionContainerWraper<TSessionData> : ISessionContainer<TSessionData>
    {
        IAsyncSessionContainer<TSessionData> _asyncSessionContainer;

        public AsyncToSyncSessionContainerWraper(IAsyncSessionContainer<TSessionData> asyncSessionContainer)
        {
            _asyncSessionContainer = asyncSessionContainer;
        }

        public IAppSession<TSessionData> GetSessionByID(string sessionID)
        {
            return _asyncSessionContainer.GetSessionByIDAsync(sessionID).Result;
        }

        public int GetSessionCount()
        {
            return _asyncSessionContainer.GetSessionCountAsync().Result;
        }

        public IEnumerable<IAppSession<TSessionData>> GetSessions(Predicate<IAppSession<TSessionData>> critera)
        {
            return _asyncSessionContainer.GetSessionsAsync(critera).Result;
        }

        public IEnumerable<TAppSession> GetSessions<TAppSession>(Predicate<TAppSession> critera) where TAppSession : IAppSession<TSessionData>
        {
            return _asyncSessionContainer.GetSessionsAsync<TAppSession>(critera).Result;
        }
    }
}