
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IMiddleware
    {
        int Order { get; }

        void Start(IServer server);

        void Shutdown(IServer server);

        ValueTask<bool> RegisterSession<TSessionData>(IAppSession<TSessionData> session);
    }
}