using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public class DelegatePackageHandler<TSessionData,TReceivePackageInfo> : IPackageHandler<TSessionData,TReceivePackageInfo>
    {

        Func<IAppSession<TSessionData>, TReceivePackageInfo, ValueTask> _func;

        public DelegatePackageHandler(Func<IAppSession<TSessionData>, TReceivePackageInfo, ValueTask> func)
        {
            _func = func;
        }

        public async ValueTask Handle(IAppSession<TSessionData> session, TReceivePackageInfo package)
        {
            await _func(session, package);
        }
    }
}