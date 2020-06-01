using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IPackageHandler<TSessionData, TReceivePackageInfo>
        //where TReceivePackageInfo : class
    {
        ValueTask Handle(IAppSession<TSessionData> session, TReceivePackageInfo package);
    }
}