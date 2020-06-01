using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public class SyncToAsyncSessionContainerWraper<TSessionData> : IAsyncSessionContainer<TSessionData>
    {
        ISessionContainer<TSessionData> _syncSessionContainer;

        public SyncToAsyncSessionContainerWraper(ISessionContainer<TSessionData> syncSessionContainer)
        {
            _syncSessionContainer = syncSessionContainer;
        }

        public ValueTask<IAppSession<TSessionData>> GetSessionByIDAsync(string sessionID)
        {
            return new ValueTask<IAppSession<TSessionData>>(_syncSessionContainer.GetSessionByID(sessionID));
        }

        public ValueTask<int> GetSessionCountAsync()
        {
            return new ValueTask<int>(_syncSessionContainer.GetSessionCount());
        }

        public ValueTask<IEnumerable<IAppSession<TSessionData>>> GetSessionsAsync(Predicate<IAppSession<TSessionData>> critera = null)
        {
            return new ValueTask<IEnumerable<IAppSession<TSessionData>>>(_syncSessionContainer.GetSessions(critera));
        }

        public ValueTask<IEnumerable<TAppSession>> GetSessionsAsync<TAppSession>(Predicate<TAppSession> critera = null) where TAppSession : IAppSession<TSessionData>
        {
            return new ValueTask<IEnumerable<TAppSession>>(_syncSessionContainer.GetSessions<TAppSession>(critera));
        }
    }
}