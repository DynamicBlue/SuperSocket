using System;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IPackageHandler<TReceivePackageInfo>
        //where TReceivePackageInfo : class
    {
        ValueTask Handle(IAppSession session, TReceivePackageInfo package);
    }
}